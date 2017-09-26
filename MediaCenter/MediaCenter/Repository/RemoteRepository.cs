using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaCenter.Helpers;
using MediaCenter.Media;
using MediaCenter.Sessions;

namespace MediaCenter.Repository
{
    public class RemoteRepository : IRepository
    {
        private const string MediaFileExtension = ".mcd";

        private readonly string _localStoreFilePath;
        private readonly string _remoteStore;
        private readonly string _localCachePath;
        private DateTime _lastSyncFromRemote;
        private List<MediaItem> _catalog;
        private ConcurrentDictionary<string, byte[]> _buffer;
        private CancellationTokenSource _bufferCancellationTokenSource;
        private bool _prefetchingInProgress;

        public IEnumerable<MediaItem> Catalog => _catalog;

        // TODO: straightforward approach, might need optimization
        public IEnumerable<string> Tags => _catalog.SelectMany(x => x.Tags).Distinct();

        public RemoteRepository(string remoteStore, string localStoreFilePath, string localCachePath)
        {
            _remoteStore = remoteStore;
            _localStoreFilePath = localStoreFilePath;
            _localCachePath = localCachePath;
            _catalog = new List<MediaItem>();
            _buffer = new ConcurrentDictionary<string, byte[]>();
        }

        public async Task Initialize()
        {
            await ReadLocalStore();
            await SynchronizeFromRemoteStore();
        }

        private async Task ReadLocalStore()
        {
            if (!File.Exists(_localStoreFilePath))
            {
                _catalog = new List<MediaItem>();
                await UpdateLocalStore();
                return;
            }

            var infoList = await IOHelper.OpenObject<List<MediaInfo>>(_localStoreFilePath);
            _catalog = infoList.Select(i => CreateMediaItem(i)).ToList();
        }

        private async Task UpdateLocalStore()
        {
            await IOHelper.SaveObject(Catalog.Select(i => new MediaInfo(i)).ToList(), _localStoreFilePath);
        }

        public async Task SynchronizeFromRemoteStore()
        {
            
            var remoteStoreDirectory = new DirectoryInfo(_remoteStore);
            var remoteStoreMediaFiles = remoteStoreDirectory.GetFiles("*" + MediaFileExtension);

            // find and delete all items in the local store that are no longer in the remote store 
            var deleteList = Catalog.Where(item =>
                remoteStoreMediaFiles.All(f => f.Name.ToLower() != item.Name + MediaFileExtension)).ToList();
            foreach (var mediaItem in deleteList)
            {
                _catalog.Remove(mediaItem);
            }

            // read all remote media files that were created or updates since the last sync
            var newLastSyncedDate = DateTime.Now;
            foreach (var file in remoteStoreMediaFiles.Where(f => f.LastWriteTime >= _lastSyncFromRemote))
            {
                var info = await IOHelper.OpenObject<MediaInfo>(file.FullName);
                var item = CreateMediaItem(info);
                var existingItem = Catalog.FirstOrDefault(x => x.Name == item.Name);
                if (existingItem == null) // new item
                {
                    _catalog.Add(item);
                }
                else
                {
                    existingItem.UpdateFrom(item);
                }
            }

            await UpdateLocalStore();
            _lastSyncFromRemote = newLastSyncedDate;
        }

        public async Task SaveNewItems(IEnumerable<KeyValuePair<string,MediaItem>> newItems) // list of (filePath, Item) pairs
        {
            foreach (var newItemPair in newItems.Where(x => x.Value.Status == MediaItemStatus.Staged))
            {
                var filePath = newItemPair.Key;
                var newItem = newItemPair.Value;
                var originalName = newItem.Name;
                bool wasRenamed = false;
                
                try
                {
                    if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(newItem.Name))
                    {
                        newItem.Status = MediaItemStatus.Error;
                        continue;
                    }

                    newItem.Name = CreateUniqueName(originalName);

                    int i = 1;
                    while (_catalog.Any(x => x.Name == newItem.Name))
                    {
                        wasRenamed = true;
                        newItem.Name = originalName + "_" + i++;
                    }

                    // add to local store
                    _catalog.Add(newItem);

                    // add to remote store
                    var mediaItemFilename = Path.Combine(_remoteStore, newItem.Name + Path.GetExtension(filePath));
                    await IOHelper.CopyFile(filePath, mediaItemFilename);
                    await IOHelper.SaveBytes(newItem.Thumbnail, ItemNameToThumbnailFilename(newItem.Name));
                    await IOHelper.SaveObject(new MediaInfo(newItem), ItemNameToInfoFilename(newItem.Name));

                    newItem.Status = MediaItemStatus.Saved;
                }
                catch (Exception e)
                {
                    if (_catalog.Contains(newItem))
                        _catalog.Remove(newItem); // remove from local catalog to avoid discrepancy between catalog and store
                    if (wasRenamed)
                        newItem.Name = originalName; // reset name change because the item was not saved to the store
                    newItem.Status = MediaItemStatus.Error;
                    continue;
                }
            }

            await UpdateLocalStore();
        }

        private string CreateUniqueName(string originalName)
        {
            var name = originalName;
            
            // if the name was already altered for uniqueness during staging, to avid getting ..._1_1 situations
            // we strip down the previous changes and start again from teh base name to create uniqueness in the catalog
            if (name.Contains("_"))
            {
                name = name.Substring(0, name.IndexOf("_", StringComparison.InvariantCulture));
            }

            var newName = name;
            var i = 1;
            while (_catalog.Any(x => x.Name == newName))
            {
                newName = name + "_" + i++;
            }

            return newName;
        }

        public async Task DeleteItem(string name)
        {
            var item = _catalog.First(x => x.Name == name);
            try
            {
                _catalog.Remove(item);
                await IOHelper.DeleteFile(ItemNameToContentFilePath(name));
                await IOHelper.DeleteFile(ItemNameToThumbnailFilename(name));
                await IOHelper.DeleteFile(ItemNameToInfoFilename(name));
                await UpdateLocalStore();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<byte[]> GetThumbnail(string name)
        {
            var thumbnailFilename = Path.Combine(_remoteStore, name + "_T.jpg");
            return await IOHelper.OpenBytes(thumbnailFilename);
        }

        public async Task<byte[]> GetFullImage(string name, IEnumerable<string> prefetch)
        {
            Task<byte[]> imageLoadingTask = null;
            byte[] result = null;

            if (_buffer.ContainsKey(name))
            {
                Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss tt ss.fff")} | Image {name} was in prefetch buffer");
                result = _buffer[name];
            }
            else
            {
                var imagePath = ItemNameToContentFilePath(name);
                if (!string.IsNullOrEmpty(imagePath))
                    imageLoadingTask = IOHelper.OpenBytes(imagePath);
            }

            // clean up buffer and decide which images need to be fetched
            var prefetchList = prefetch.ToList();
            var deleteFromBufferList = _buffer.Keys.Where(x => x != name && !prefetchList.Contains(x));
            foreach (var deleteItem in deleteFromBufferList)
            {
                byte[] value;
                _buffer.TryRemove(deleteItem, out value);
            }
            var itemsToBeFetched = prefetchList.Where(itemToBeBuffered => !_buffer.ContainsKey(itemToBeBuffered)).ToList();

            // start prefetch sequence
            if (itemsToBeFetched.Any())
            {
                // cancel any previous prefetching action still in progress
                if (_prefetchingInProgress && _bufferCancellationTokenSource != null)
                {
                    _bufferCancellationTokenSource.Cancel();
                }

                var prefetchingSequence = Observable.Create<KeyValuePair<string, byte[]>>(
                async (observer, token) =>
                {
                    _prefetchingInProgress = true;
                    foreach (var bufferItemName in itemsToBeFetched)
                    {
                        token.ThrowIfCancellationRequested();
                        var file = ItemNameToContentFilePath(bufferItemName);
                        if (string.IsNullOrEmpty(file))
                            continue;

                        var bytes = await IOHelper.OpenBytes(file);
                        token.ThrowIfCancellationRequested();
                        observer.OnNext(new KeyValuePair<string, byte[]>(bufferItemName, bytes));
                    }
                });
                _bufferCancellationTokenSource = new CancellationTokenSource();
                prefetchingSequence.Subscribe(
                    pair =>
                    {
                        _buffer[pair.Key] = pair.Value;
                        Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss tt ss.fff")} | Prefetched { pair.Key}");
                    },
                    () => { _prefetchingInProgress = false; },
                    _bufferCancellationTokenSource.Token);

            }

            if (imageLoadingTask != null)
                result = await imageLoadingTask;

            return result;
        }

        public async Task LoadImageToCache(string name)
        {
            var imagePath = Directory.GetFiles(_remoteStore, $"{name}.*").FirstOrDefault();
            if (string.IsNullOrEmpty(imagePath))
                return;

            // check if image is present in cache, if not, copy to cache
            var imageCachePath = Path.Combine(_localCachePath, Path.GetFileName(imagePath));
            if (!File.Exists(imageCachePath))
                await IOHelper.CopyFile(imagePath, imageCachePath);
            // TODO: cache cleanup
        }

        public async Task SaveItemInfo(string name)
        {
            var item = _catalog.First(i => i.Name == name);
            
            await IOHelper.SaveObject(new MediaInfo(item), ItemNameToInfoFilename(name));
            await UpdateLocalStore();
            _lastSyncFromRemote = DateTime.Now; // TODO: solve concurrent access issue
        }

        public async Task SaveItemContent(string name)
        {
            var content = Catalog.FirstOrDefault(x => x.Name == name)?.Content;
            if(content != null)
                await IOHelper.SaveBytes(content, ItemNameToContentFilePath(name));
            if (_buffer.ContainsKey(name))
                _buffer[name] = content;
        }

        public async Task SaveItemThumbnail(string name)
        {
            var thumbnail = Catalog.FirstOrDefault(x => x.Name == name)?.Thumbnail;
            if (thumbnail != null)
                await IOHelper.SaveBytes(thumbnail, ItemNameToThumbnailFilename(name));
        }

        private string ItemNameToContentFilePath(string name)
        {
            return Directory.GetFiles(_remoteStore, $"{name}.*").FirstOrDefault();
        }
        private string ItemNameToThumbnailFilename(string name)
        {
            return Path.Combine(_remoteStore, name + "_T.jpg");
        }
        private string ItemNameToInfoFilename(string name)
        {
            return Path.Combine(_remoteStore, name + MediaFileExtension);
        }

        private MediaItem CreateMediaItem(MediaInfo mediaInfo)
        {
            var mediaItem = mediaInfo.ToMediaItem();
            if (mediaItem.MediaType == MediaType.Video)
            {
                mediaItem.ContentUri = new Uri(ItemNameToContentFilePath(mediaItem.Name));
            }
            return mediaItem;
        }


    }
}
