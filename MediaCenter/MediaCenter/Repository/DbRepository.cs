﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MediaCenter.Helpers;
using MediaCenter.Media;
using MediaCenter.Sessions.Filters;
using MediaCenter.Sessions.Staging;

namespace MediaCenter.Repository
{
    public class DbRepository : IRepository, ICacheRepository
    {
        private readonly string _mediaFolderPath;
        private readonly string _thumbnailFolderPath;
        private readonly Database _database;

        private readonly ConcurrentDictionary<string, byte[]> _buffer;
        private CancellationTokenSource _bufferCancellationTokenSource;
        private bool _prefetchInProgress;

        private readonly ICacheRepository _cacheRepository;
        private readonly List<Task> _backgroundTasks;

        public static bool CheckRepositoryConnection(string repoPath)
        {
            return File.Exists(Path.Combine(repoPath, "db", "mc.db3"));
        }

        public DbRepository(string repoPath, ICacheRepository cache = null)
        {
            _backgroundTasks = new List<Task>();

            Location = new Uri(repoPath);
            var dbPath = Path.Combine(repoPath, "db", "mc.db3");
            var mediaFolderPath = Path.Combine(repoPath, "media");
            var thumbnailFolderPath = Path.Combine(repoPath, "thumbnails");

            _database = new Database(dbPath);
            _mediaFolderPath = mediaFolderPath;
            if (!Directory.Exists(mediaFolderPath))
                Directory.CreateDirectory(mediaFolderPath);

            _thumbnailFolderPath = thumbnailFolderPath;
            if (!Directory.Exists(thumbnailFolderPath))
                Directory.CreateDirectory(thumbnailFolderPath);

            _buffer = new ConcurrentDictionary<string, byte[]>();

            _cacheRepository = cache;
        }

        public event EventHandler CollectionChanged;
        public event EventHandler StatusChanged;

        public List<string> Tags { get; set; }

        public async Task Initialize()
        {
            Tags = (await _database.GetAllTags()).ToList();
            if (_cacheRepository != null)
            {
                await _cacheRepository.Initialize();
                var favorites = await _database.GetFilteredItemList(new List<Filter>
                    {new FavoriteFilter {FavoriteSetting = FavoriteFilter.FavoriteOption.OnlyFavorite}});
                await _cacheRepository.SynchronizeCache(favorites.Select(x =>
                    new Tuple<MediaItem, string, string>(x, GetMediaPath(x), GetThumbnailPath(x))).ToList());
            }

            ImageCount = await _database.GetFilteredItemCount(new List<Filter>{new MediaTypeFilter{MediaType = MediaType.Image}});
            VideoCount = await _database.GetFilteredItemCount(new List<Filter> { new MediaTypeFilter{ MediaType = MediaType.Video} });
            CollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task SaveNewItem(StagedItem newItem)
        {
            if (string.IsNullOrEmpty(newItem.FilePath))
            {
                newItem.Status = MediaItemStatus.Error;
                newItem.StatusMessage = "File path is missing";
                return;
            }
            
            var originalName = newItem.Name;
            newItem.Name = await CreateUniqueName(newItem);
            newItem.ContentFileName = newItem.Name + Path.GetExtension(newItem.FilePath);
            var mediaItemFilePath = GetMediaPath(newItem);
            var thumbnailFilePath = GetThumbnailPath(newItem);

            try
            {
                await IOHelper.CopyFile(newItem.FilePath, mediaItemFilePath);
                await IOHelper.SaveBytes(newItem.Thumbnail, GetThumbnailPath(newItem));
                await _database.AddMediaItem(newItem);
                newItem.Status = MediaItemStatus.Saved;
                foreach (var newTag in newItem.Tags.Where(x => !Tags.Contains(x)))
                {
                    Tags.Add(newTag);
                }

                if (newItem.MediaType == MediaType.Image)
                    ImageCount++;
                else
                    VideoCount++;
                CollectionChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                newItem.Name = originalName;
                newItem.ContentFileName = "";
                newItem.Status = MediaItemStatus.Error;
                newItem.StatusMessage = e.Message;

                await IOHelper.DeleteFile(mediaItemFilePath);
                await IOHelper.DeleteFile(thumbnailFilePath);
            }
        }

        private string CreateBaseName(MediaItem item)
        {
            return item.DateTaken.ToString("yyyyMMddHHmmss");
        }

        private async Task<string> CreateUniqueName(MediaItem item)
        {
            var baseName = CreateBaseName(item);
            
            var nameClashes = await _database.GetNameClashes(baseName);
            if (!nameClashes.Any())
                return baseName;

            var cnt = 1;
            string newName;
            while (nameClashes.Contains(newName = $"{baseName}_{cnt++}")) { }
            return newName;
        }

        private string GetMediaPath(MediaItem item)
        {
            return Path.Combine(_mediaFolderPath, item.ContentFileName);
        }
        private string GetThumbnailPath(MediaItem item)
        {
            return Path.Combine(_thumbnailFolderPath, item.Name + "_T.jpg");
        }

        private string GetOriginalMediaPath(MediaItem item)
        {
            var mediaPath = GetMediaPath(item);
            return Path.Combine(Path.GetDirectoryName(mediaPath)??"", Path.GetFileNameWithoutExtension(mediaPath) + "_o" + Path.GetExtension(mediaPath)??"");
        }

        public Uri GetContentUri(MediaItem item)
        {
            return new Uri(GetMediaPath(item));
        }

        public async Task DeleteItem(MediaItem item)
        {
            try
            {
                await IOHelper.DeleteFile(GetMediaPath(item));
                await IOHelper.DeleteFile(GetThumbnailPath(item));
                await _database.DeleteMediaItem(item);

                if (item.MediaType == MediaType.Image)
                    ImageCount--;
                else
                    VideoCount--;
                CollectionChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                item.Status = MediaItemStatus.Error;
                item.StatusMessage = $"Delete failed: {e.Message}";
            }
        }

        public async Task<byte[]> GetThumbnail(MediaItem item)
        {
            return await IOHelper.OpenBytes(GetThumbnailPath(item));
        }

        public async Task<byte[]> GetFullImage(MediaItem item, IEnumerable<MediaItem> prefetch = null)
        {
            Task<byte[]> imageLoadingTask = null;
            byte[] result = null;

            if (item == null)
                return null;

            if (_buffer.ContainsKey(item.Name))
            {
                Debug.WriteLine($"{DateTime.Now:HH:mm:ss.fff)} | Image {item.Name} was in prefetch buffer");
                result = _buffer[item.Name];
            }
            else
            {
                var imagePath = GetMediaPath(item);
                if (!string.IsNullOrEmpty(imagePath))
                    imageLoadingTask = IOHelper.OpenBytes(imagePath);
            }

            //// clean up buffer and decide which images need to be fetched
            //var prefetchList = prefetch.ToList();
            //var deleteFromBufferList = _buffer.Keys.Where(x => x != item.Name && !prefetchList.Select(y => y.Name).Contains(x));
            //foreach (var deleteItem in deleteFromBufferList)
            //{
            //    _buffer.TryRemove(deleteItem, out var value);
            //}
            //var itemsToBeFetched = prefetchList.Where(x => !_buffer.ContainsKey(x.Name)).ToList();

            //// start prefetch sequence
            //if (itemsToBeFetched.Any())
            //{
            //    // cancel any prefetch action still in progress
            //    if (_prefetchInProgress)
            //    {
            //        _bufferCancellationTokenSource?.Cancel();
            //    }

            //    var prefetchSequence = Observable.Create<KeyValuePair<string, byte[]>>(
            //    async (observer, token) =>
            //    {
            //        _prefetchInProgress = true;
            //        foreach (var bufferItem in itemsToBeFetched)
            //        {
            //            // before fetching each item, check if cancellation was requested
            //            token.ThrowIfCancellationRequested();
            //            var bytes = await IOHelper.OpenBytes(GetMediaPath(bufferItem));
            //            token.ThrowIfCancellationRequested();
            //            observer.OnNext(new KeyValuePair<string, byte[]>(bufferItem.Name, bytes));
            //        }
            //    });
            //    _bufferCancellationTokenSource = new CancellationTokenSource();
            //    prefetchSequence.Subscribe(
            //        pair =>
            //        {
            //            _buffer[pair.Key] = pair.Value;
            //            Debug.WriteLine($"{DateTime.Now:HH:mm:ss tt ss.fff} | Prefetched { pair.Key}");
            //        },
            //        () => { _prefetchInProgress = false; },
            //        _bufferCancellationTokenSource.Token);
            //}

            if (imageLoadingTask != null)
                result = await imageLoadingTask;

            return result;
        }

        public async Task SaveItem(MediaItem item)
        {
            if (_cacheRepository != null)
            {
                var wasFavorite = await _database.IsFavorite(item.Name);
                if (item.Favorite)
                {
                    if(wasFavorite)
                        AddToBackgroundTasks(_cacheRepository.UpdateCache(item)); 
                    else
                        AddToBackgroundTasks(_cacheRepository.AddToCache(item, GetMediaPath(item), GetThumbnailPath(item)));
                }

                if (!item.Favorite && wasFavorite)
                {
                    AddToBackgroundTasks(_cacheRepository.RemoveFromCache(item));
                }
            }

            await _database.UpdateMediaItem(item);
            foreach (var newTag in item.Tags.Where(x => !Tags.Contains(x)))
            {
                Tags.Add(newTag);
            }
        }

        public async Task<byte[]> GetOriginalFullImage(MediaItem item)
        {
            return await IOHelper.OpenBytes(GetOriginalMediaPath(item));
        }

        public async Task SaveEditedImage(MediaItem item, byte[] image)
        {
            var currentImage = GetMediaPath(item);

            var originalImage = GetOriginalMediaPath(item);
            if(!File.Exists(originalImage))
                File.Move(currentImage, GetOriginalMediaPath(item));

            await IOHelper.SaveBytes(image, currentImage);
        }

        public async Task SaveEditedThumbnail(MediaItem item, byte[] thumbnail)
        {
            await IOHelper.SaveBytes(thumbnail, GetThumbnailPath(item));
        }

        public async Task<bool> IsDuplicate(StagedItem stagedItem)
        {
            var duplicateCandidates = await _database.GetNameClashes(CreateBaseName(stagedItem)); // duplicate --> same date --> name clash

            foreach (var duplicateCandidateName in duplicateCandidates)
            {
                var item = await  _database.GetItemByName(duplicateCandidateName);
                var newFile = new FileInfo(stagedItem.FilePath);
                var existingFile = File.Exists(GetOriginalMediaPath(item))
                    ? new FileInfo(GetOriginalMediaPath(item))
                    : new FileInfo(GetMediaPath(item));
                if (newFile.Length == existingFile.Length)
                {
                    var newBytes = await IOHelper.OpenBytes(newFile.FullName);
                    var existingBytes = await IOHelper.OpenBytes(existingFile.FullName);
                    if(newBytes.SequenceEqual(existingBytes))
                        return true;
                }
            }

            return false;
        }

        private void AddToBackgroundTasks(Task task)
        {
            task.ContinueWith((t) => _backgroundTasks.Remove(t));
            _backgroundTasks.Add(task);
        }

        public void Close()
        {
            while(_backgroundTasks.Any(x => x != null && !x.IsCompleted))
                Thread.Sleep(500);
        }

        public int ImageCount { get; private set; }
        public int VideoCount { get; private set; }

        public Uri Location {get; private set; }
        public async Task SaveContentToFile(MediaItem item, string filePath)
        {
            await IOHelper.CopyFile(GetMediaPath(item), filePath);
        }

        public async Task SaveMultipleContentToFolder(List<MediaItem> items, string folderPath)
        {
            foreach (var item in items)
            {
                await IOHelper.CopyFile(GetMediaPath(item), Path.Combine(folderPath, item.ContentFileName));
            }
        }

        public string StatusMessage { get; private set; }

        public async Task<List<MediaItem>> GetQueryItems(IEnumerable<Filter> filters)
        {
            return await _database.GetFilteredItemList(filters); ;
        }

        public async Task<int> GetQueryCount(IEnumerable<Filter> filters)
        {
            return await _database.GetFilteredItemCount(filters);
        }

        public async Task AddToCache(MediaItem item, string filePath, string thumbnailPath)
        {
            if(await _database.ItemExists(item.Name))
                return;

            await IOHelper.CopyFile(filePath, GetMediaPath(item));
            await IOHelper.CopyFile(thumbnailPath, GetThumbnailPath(item));
            await _database.AddMediaItem(item);
        }

        public async Task RemoveFromCache(MediaItem item)
        {
            await DeleteItem(item);
        }

        public async Task SynchronizeCache(List<Tuple<MediaItem, string, string>> syncItems)
        {
            var localItems = await _database.GetFilteredItemList(new List<Filter>());

            var syncItemNames =  syncItems.Select(x => x.Item1.Name);
            var localItemsToRemove = localItems.Where(x => !syncItemNames.Contains(x.Name)).ToList();
            foreach (var item in localItemsToRemove)
            {
                await RemoveFromCache(item);
            }

            foreach (var tuple in syncItems)
            {
                var localItem = localItems.FirstOrDefault(x => x.Name == tuple.Item1.Name);
                if (localItem == null)
                {
                    await AddToCache(tuple.Item1, tuple.Item2, tuple.Item3);
                }
                else
                {
                    localItem.UpdateFrom(tuple.Item1); // don't directly save the sync item as the id is autogenerated and will not match
                    await _database.UpdateMediaItem(localItem);
                }
            }
        }

        public async Task UpdateCache(MediaItem item)
        {
            var localItem = await _database.GetItemByName(item.Name);
            if (localItem == null)
                return;

            localItem.UpdateFrom(item); // don't directly save the item as the id is autogenerated and will not match
            await _database.UpdateMediaItem(localItem);
        }
    }
}
