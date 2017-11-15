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
using MediaCenter.Sessions.Staging;

namespace MediaCenter.Repository
{
    public class RemoteRepository : IRepository
    {
        private const string MediaFileExtension = ".mcd";

        private readonly string _localStoreFilePath;
        private readonly string _lastSyncFilePath;
        private readonly string _remoteStore;
        private readonly string _localCachePath;
        private DateTime _lastSyncFromRemote;
        private List<MediaItem> _catalog;
        private ConcurrentDictionary<string, byte[]> _buffer;
        private CancellationTokenSource _bufferCancellationTokenSource;
        private bool _prefetchingInProgress;

        public event EventHandler CollectionChanged;
        public event EventHandler StatusChanged;

        public IEnumerable<MediaItem> Catalog => _catalog;

        // TODO: straightforward approach, might need optimization
        public IEnumerable<string> Tags => _catalog.SelectMany(x => x.Tags).Distinct();

        public Uri Location => new System.Uri(_remoteStore);

        private string _statusMessage;
        public string StatusMessage
        {
            get { return _statusMessage; }
            private set
            {
                _statusMessage = value;
                RaiseStatusChanged();
            }
        }

        public RemoteRepository(string remoteStore, string localStoreFilePath, string localCachePath)
        {
            _remoteStore = remoteStore;
            _localStoreFilePath = localStoreFilePath;
            _lastSyncFilePath = Path.ChangeExtension(_localStoreFilePath, ".mct");
            _localCachePath = localCachePath;
            _catalog = new List<MediaItem>();
            _buffer = new ConcurrentDictionary<string, byte[]>();
        }

        public async Task Initialize()
        {
            StatusMessage = "Reading the local store";
            await ReadLocalStore();
            StatusMessage = "Synchronizing with the remote store";
            await SynchronizeFromRemoteStore();
            RaiseCollectionChangedEvent();
            StatusMessage = "";
        }        

        private async Task ReadLocalStore()
        {
            if (!File.Exists(_localStoreFilePath))
            {
                _catalog = new List<MediaItem>();
                await UpdateLocalStore();
                await UpdateLastSyncDate(DateTime.MinValue.AddDays(1));
                return;
            }

            var infoList = await IOHelper.OpenObject<List<MediaInfo>>(_localStoreFilePath);
            _catalog = infoList.Select(i => CreateMediaItem(i)).ToList();

            if (!File.Exists(_lastSyncFilePath))
            {
                await UpdateLastSyncDate(DateTime.MinValue.AddDays(1));
            }
            else
            {
                _lastSyncFromRemote = await IOHelper.OpenObject<DateTime>(_lastSyncFilePath);
            }
        }

        private async Task UpdateLocalStore()
        {
            await IOHelper.SaveObject(Catalog.Select(i => new MediaInfo(i)).ToList(), _localStoreFilePath);
        }

        private async Task UpdateLastSyncDate(DateTime date)
        {
            _lastSyncFromRemote = date;
            await IOHelper.SaveObject(_lastSyncFromRemote, _lastSyncFilePath);
        }

        public async Task SynchronizeFromRemoteStore()
        {
            var newLastSyncedDate = DateTime.Now;
            var remoteStoreMediaFiles = await IOHelper.GetFiles(_remoteStore, "*" + MediaFileExtension);
            
            // dele all items from the catalog that are not in teh remote list anymore (have been deleted since the last sync)
            var deleteList = Catalog.Select(x => x.Name).Except(remoteStoreMediaFiles.Select(x => Path.GetFileNameWithoutExtension(x.Name))).ToList();
            foreach (var name in deleteList)
            {
                _catalog.Remove(GetItem(name));
            }

            // read all remote media files that were created or updates since the last sync
            foreach (var file in remoteStoreMediaFiles.Where(f => f.LastWriteTime >= _lastSyncFromRemote))
            {
                var info = await IOHelper.OpenObject<MediaInfo>(file.FullName);
                var newItem = CreateMediaItem(info);
                var existingItem = GetItem(newItem.Name);
                if (existingItem == null) // new item
                {
                    _catalog.Add(newItem);
                }
                else
                {
                    existingItem.UpdateFrom(newItem);
                }
            }

            await UpdateLocalStore();
            await UpdateLastSyncDate(newLastSyncedDate);
        }

        public async Task SaveNewItems(IEnumerable<StagedItem> newItems) 
        {
            var itemsToSave = newItems.Where(x => x.Status == MediaItemStatus.Staged).ToList();
            var count = itemsToSave.Count;
            var i = 1;

            foreach (var newItem in itemsToSave)
            {
                StatusMessage = $"Saving item {i++} of {count}";
                var originalName = newItem.Name;
                
                try
                {                    
                    if (string.IsNullOrEmpty(newItem.FilePath) || string.IsNullOrEmpty(newItem.Name))
                    {
                        newItem.Status = MediaItemStatus.Error;
                        continue;
                    }

                    newItem.Name = CreateUniqueName(originalName);
                    newItem.ContentFileName = newItem.Name + Path.GetExtension(newItem.FilePath);
                    _catalog.Add(newItem);

                    // save in remote store
                    var mediaItemFilePath = Path.Combine(_remoteStore, newItem.ContentFileName);
                     
                    await IOHelper.SaveObject(new MediaInfo(newItem), ItemNameToInfoFilePath(newItem.Name));
                    await IOHelper.CopyFile(newItem.FilePath, mediaItemFilePath);
                    await IOHelper.SaveBytes(newItem.Thumbnail, ItemNameToThumbnailFilePath(newItem.Name));
                    
                    newItem.ContentUri = new Uri(mediaItemFilePath);
                    newItem.Status = MediaItemStatus.Saved;                    
                }
                catch (Exception e)
                {
                    if (_catalog.Contains(newItem))
                        _catalog.Remove(newItem); // remove from local catalog to avoid discrepancy between catalog and store
                    // TODO: cleanup files that were already saved
                    newItem.Name = originalName; // reset name change because the item was not saved to the store
                    newItem.Status = MediaItemStatus.Error;
                    continue;
                }
            }
            StatusMessage = "Updating the local store";
            await UpdateLocalStore();
            StatusMessage = "";
            RaiseCollectionChangedEvent();
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
            StatusMessage = "Deleting item";            
            try
            {
                var item = _catalog.First(x => x.Name == name);
                var contentFilePath = FileNameToRemoteFilePath(item.ContentFileName);
                var thumbnailFilePath = ItemNameToThumbnailFilePath(name);
                var infoFileName = ItemNameToInfoFilePath(name);

                _catalog.Remove(item);
                await IOHelper.DeleteFile(contentFilePath);
                await IOHelper.DeleteFile(thumbnailFilePath);
                await IOHelper.DeleteFile(infoFileName);
                await UpdateLocalStore();

                RaiseCollectionChangedEvent();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                StatusMessage = "";
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

            var item = GetItem(name);
            if (item == null)
                return null;

            if (_buffer.ContainsKey(name))
            {
                Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss tt ss.fff")} | Image {name} was in prefetch buffer");
                result = _buffer[name];
            }
            else
            {
                var imagePath = FileNameToRemoteFilePath(item.ContentFileName);
                if (!string.IsNullOrEmpty(imagePath))
                    imageLoadingTask = IOHelper.OpenBytes(imagePath);
            }

            // clean up buffer and decide which images need to be fetched
            var prefetchList = prefetch.ToList();
            var deleteFromBufferList = _buffer.Keys.Where(x => x != name && !prefetchList.Contains(x));
            foreach (var deleteItem in deleteFromBufferList)
            {
                _buffer.TryRemove(deleteItem, out var value);
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
                        // before fetching each item, check if cancellation was requested
                        token.ThrowIfCancellationRequested();

                        var file = ItemNameToRemoteFilePath(bufferItemName);
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

        public async Task SaveItem(string name)
        {
            var item = GetItem(name);
            if (item == null)
                return;

            if (item.IsInfoDirty)
            {
                await SaveItemInfo(item);
                item.IsInfoDirty = false;
            }

            if (item.IsContentDirty)
            {
                await SaveItemContent(item);
                item.IsContentDirty = false;
            }

            if (item.IsThumbnailDirty)
            {
                await SaveItemThumbnail(item);
                item.IsThumbnailDirty = false;
            }
        }

        public async Task SaveItemInfo(MediaItem item)
        {
            await IOHelper.SaveObject(new MediaInfo(item), ItemNameToInfoFilePath(item.Name));
            await UpdateLocalStore();
            await UpdateLastSyncDate(DateTime.Now); // TODO: this approach does not allow concurrent access
        }

        public async Task SaveItemContent(MediaItem item)
        {   
            await IOHelper.SaveBytes(item.Content, FileNameToRemoteFilePath(item.ContentFileName));
            // make sure that any buffered version of the content is also updated 
            if (_buffer.ContainsKey(item.Name))
                _buffer[item.Name] = item.Content;
        }

        public async Task SaveItemThumbnail(MediaItem item)
        {
            var thumbnail = item.Thumbnail;
            if (thumbnail != null)
                await IOHelper.SaveBytes(thumbnail, ItemNameToThumbnailFilePath(item.Name));
        }

        private string FileNameToRemoteFilePath(string fileName)
        {
            return string.IsNullOrEmpty(fileName) ? null : Path.Combine(_remoteStore, fileName);
        }
        public string ItemNameToRemoteFilePath(string itemName)
        {
            return FileNameToRemoteFilePath(GetItem(itemName)?.ContentFileName);
        }
        private string ItemNameToThumbnailFilePath(string itemName)
        {
            return Path.Combine(_remoteStore, itemName + "_T.jpg");
        }
        private string ItemNameToInfoFilePath(string itemName)
        {
            return Path.Combine(_remoteStore, itemName + MediaFileExtension);
        }

        private MediaItem GetItem(string name)
        {
            return Catalog.FirstOrDefault(x => x.Name == name);
        }

        private MediaItem CreateMediaItem(MediaInfo mediaInfo)
        {
            var mediaItem = mediaInfo.ToMediaItem();
            if (mediaItem.MediaType == MediaType.Video && !string.IsNullOrEmpty(mediaItem.ContentFileName))
            {
                mediaItem.ContentUri = new Uri(FileNameToRemoteFilePath(mediaItem.ContentFileName));
            }
            return mediaItem;
        }

        private void RaiseCollectionChangedEvent()
        {
            CollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseStatusChanged()
        {
            StatusChanged?.Invoke(this, EventArgs.Empty);     
        }

        public async Task SaveContentToFile(MediaItem item, string filePath)
        {
            await IOHelper.CopyFile(FileNameToRemoteFilePath(item.ContentFileName), filePath);
        }

        public async Task SaveContentToFolder(List<MediaItem> items, string folderPath)
        {
            foreach (var item in items)
            {
                await IOHelper.CopyFile(FileNameToRemoteFilePath(item.ContentFileName),
                    Path.Combine(folderPath, item.ContentFileName));
            }
        }

    }
}
