﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaCenter.Helpers;
using MediaCenter.Media;
using MediaCenter.Sessions.Staging;

namespace MediaCenter.Repository
{
    public class DbRepository : IRepository
    {
        private readonly string _mediaFolderPath;
        private readonly string _thumbnailFolderPath;
        private readonly Database _database;

        private ConcurrentDictionary<string, byte[]> _buffer;
        private CancellationTokenSource _bufferCancellationTokenSource;
        private bool _prefetchingInProgress;

        public DbRepository(string dbPath, string mediaFolderPath, string thumbnailFolderPath)
        {
            _database = new Database(dbPath);
            _mediaFolderPath = mediaFolderPath;
            _thumbnailFolderPath = thumbnailFolderPath;
        }

        public event EventHandler CollectionChanged;
        public event EventHandler StatusChanged;
        public IEnumerable<MediaItem> Catalog { get; }
        public IEnumerable<string> Tags { get; }
        public Task Initialize()
        {
            return Task.CompletedTask;
        }

        public async Task SaveNewItems(IEnumerable<StagedItem> newItems)
        {
            var itemsToSave = newItems.Where(x => x.Status == MediaItemStatus.Staged).ToList();
            var count = itemsToSave.Count;
            var i = 1;

            foreach (var newItem in itemsToSave)
            {
                StatusMessage = $"Saving item {i++} of {count}";

                if (string.IsNullOrEmpty(newItem.FilePath))
                {
                    newItem.Status = MediaItemStatus.Error;
                    newItem.StatusMessage = "File path is missing";
                    continue;
                }
                if (string.IsNullOrEmpty(newItem.Name))
                {
                    newItem.Status = MediaItemStatus.Error;
                    newItem.StatusMessage = "Name is missing";
                    continue;
                }

                var originalName = newItem.Name;
                newItem.Name = await CreateUniqueName(originalName);
                newItem.ContentFileName = newItem.Name + Path.GetExtension(newItem.FilePath);
                var mediaItemFilePath = GetMediaPath(newItem);
                var thumbnailFilePath = GetThumbnailPath(newItem);

                try
                {
                    await IOHelper.CopyFile(newItem.FilePath, mediaItemFilePath);
                    await IOHelper.SaveBytes(newItem.Thumbnail, GetThumbnailPath(newItem));
                    await _database.AddMediaInfo(newItem);
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
        }
        
        private async Task<string> CreateUniqueName(string originalName)
        {
            string newName = originalName;
            // if the name was already altered for uniqueness during staging, to avid getting ..._1_1 situations
            // we strip down the previous changes and start again from teh base name to create uniqueness in the catalog
            if (originalName.Contains("_"))
            {
                newName = originalName.Substring(0, originalName.IndexOf("_", StringComparison.InvariantCulture));
            }

            var nameClashes = await _database.GetNameClashes(newName);
            if (!nameClashes.Any())
                return originalName;

            var cnt = 1;
            while (nameClashes.Contains(newName = $"{originalName}_{cnt++}")) { }
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

        public async Task DeleteItem(MediaItem item)
        {
            try
            {
                await IOHelper.DeleteFile(GetMediaPath(item));
                await IOHelper.DeleteFile(GetThumbnailPath(item));
                await _database.DeleteMediaInfo(item);
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

        public async Task<byte[]> GetFullImage(MediaItem item, IEnumerable<MediaItem> prefetch)
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

            // clean up buffer and decide which images need to be fetched
            var prefetchList = prefetch.ToList();
            var deleteFromBufferList = _buffer.Keys.Where(x => x != item.Name && !prefetchList.Select(y => y.Name).Contains(x));
            foreach (var deleteItem in deleteFromBufferList)
            {
                _buffer.TryRemove(deleteItem, out var value);
            }
            var itemsToBeFetched = prefetchList.Where(x => !_buffer.ContainsKey(x.Name)).ToList();

            // start prefetch sequence
            if (itemsToBeFetched.Any())
            {
                // cancel any prefetch action still in progress
                if (_prefetchingInProgress)
                {
                    _bufferCancellationTokenSource?.Cancel();
                }

                var prefetchSequence = Observable.Create<KeyValuePair<string, byte[]>>(
                async (observer, token) =>
                {
                    _prefetchingInProgress = true;
                    foreach (var bufferItem in itemsToBeFetched)
                    {
                        // before fetching each item, check if cancellation was requested
                        token.ThrowIfCancellationRequested();
                        var bytes = await IOHelper.OpenBytes(GetMediaPath(bufferItem));
                        token.ThrowIfCancellationRequested();
                        observer.OnNext(new KeyValuePair<string, byte[]>(bufferItem.Name, bytes));
                    }
                });
                _bufferCancellationTokenSource = new CancellationTokenSource();
                prefetchSequence.Subscribe(
                    pair =>
                    {
                        _buffer[pair.Key] = pair.Value;
                        Debug.WriteLine($"{DateTime.Now:HH:mm:ss tt ss.fff} | Prefetched { pair.Key}");
                    },
                    () => { _prefetchingInProgress = false; },
                    _bufferCancellationTokenSource.Token);
            }

            if (imageLoadingTask != null)
                result = await imageLoadingTask;

            return result;
        }

        public async Task SaveItem(MediaItem item)
        {
            await _database.UpdateMediaInfo(item);
        }

        public Uri Location { get; }
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
    }
}