﻿using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.XPath;
using MediaCenter.Helpers;
using MediaCenter.Sessions;
using MediaCenter.Sessions.Staging;
using Image = System.Drawing.Image;

namespace MediaCenter.Repository
{
    public class RemoteRepository
    {
        private const string MediaFileExtension = ".mcd";

        private readonly string _localStoreFilePath;
        private readonly string _remoteStore;
        private readonly string _localCachePath;
        private DateTime _lastSyncFromRemote;
        private List<MediaInfo> _catalog;
        private Dictionary<string, byte[]> _buffer;
        private CancellationTokenSource _bufferCancellationTokenSource = new CancellationTokenSource();
        private IDisposable _bufferSubscription;
        
        public IEnumerable<MediaInfo> Catalog => _catalog;

        public RemoteRepository(string remoteStore, string localStoreFilePath, string localCachePath)
        {
            _remoteStore = remoteStore;
            _localStoreFilePath = localStoreFilePath;
            _localCachePath = localCachePath;
            _catalog = new List<MediaInfo>();
            _buffer = new Dictionary<string, byte[]>();
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
                _catalog = new List<MediaInfo>();
                await UpdateLocalStore();
                return;
            }

            _catalog = await IOHelper.OpenObject<List<MediaInfo>>(_localStoreFilePath);
        }

        private async Task UpdateLocalStore()
        {
            await IOHelper.SaveObject(Catalog, _localStoreFilePath);
        }

        public async Task SynchronizeFromRemoteStore()
        {
            var newLastSyncedDate = DateTime.Now;

            var remoteStoreDirectory = new DirectoryInfo(_remoteStore);
            var remoteStoreMediaFiles = remoteStoreDirectory.GetFiles("*" + MediaFileExtension);

            // read all remote media files that were created or updates since the last sync
            foreach (var file in remoteStoreMediaFiles.Where(f => f.LastWriteTime >= _lastSyncFromRemote))
            {
                var item = await IOHelper.OpenObject<MediaInfo>(file.FullName);
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
            // find and delete all items in the local store that are not in the remote store anymore
            var deleteList = Catalog.Where(item => 
                remoteStoreMediaFiles.All(f => f.Name.ToLower() != item.Name + MediaFileExtension)).ToList();
            foreach (var mediaItem in deleteList)
            {
                _catalog.Remove(mediaItem);
            }

            await UpdateLocalStore();
            _lastSyncFromRemote = newLastSyncedDate;
        }

        public async Task SaveStagedItems(IEnumerable<StagedItem> newItems )
        {
            foreach (var newItem in newItems)
            {
                if (string.IsNullOrEmpty(newItem.FilePath) || string.IsNullOrEmpty(newItem.Name))
                    // TODO: error handling
                    continue;

                // add to remote store
                var mediaItemFilename = Path.Combine(_remoteStore, newItem.Name + Path.GetExtension(newItem.FilePath));
                await IOHelper.CopyFile(newItem.FilePath, mediaItemFilename);

                var thumbnailFilename = Path.Combine(_remoteStore, newItem.Name + "_T.jpg");
                await IOHelper.SaveBytes(newItem.Thumbnail, thumbnailFilename);

                var descriptorFilename = Path.Combine(_remoteStore, newItem.Name + MediaFileExtension);
                await IOHelper.SaveObject(newItem.Info, descriptorFilename);
            }

            await SynchronizeFromRemoteStore();
            // yes, this causes a retrieval of objects we already have in memory, 
            // but it fixes the concurrent access issues by always having the remote being the master
            // over the local store, so it's worth it, as media files are quite small
        }

        public async Task<byte[]> GetThumbnailBytes(string name)
        {
            var thumbnailFilename = Path.Combine(_remoteStore, name + "_T.jpg");
            return await IOHelper.OpenBytes(thumbnailFilename);
        }

        public async Task<byte[]> GetFullImage(string name, IEnumerable<string> bufferList)
        {
            Task<byte[]> imageLoadingTask = null;
            byte[] result;

            if (_buffer.ContainsKey(name))
            {
                result = _buffer[name];
            }
            else
            {
                var imagePath = ItemNameToImageFilename(name);
                if (string.IsNullOrEmpty(imagePath))
                    result = null;

                imageLoadingTask = IOHelper.OpenBytes(imagePath);
            }


            var prefetchingSequence = Observable.Create<KeyValuePair<string, byte[]>>(
                async (observer, token) =>
                {
                    foreach (var bufferItemName in bufferList)
                    {
                        token.ThrowIfCancellationRequested();
                        var file = ItemNameToImageFilename(bufferItemName);
                        if (string.IsNullOrEmpty(file))
                            continue;

                        var bytes = await IOHelper.OpenBytes(file);
                        token.ThrowIfCancellationRequested();
                        observer.OnNext(new KeyValuePair<string, byte[]>(bufferItemName, bytes));
                    }
                });

            prefetchingSequence.Subscribe(
                pair => { _buffer[pair.Key] = pair.Value; },
                () => { },
                _bufferCancellationTokenSource.Token);



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

        private string ItemNameToImageFilename(string name)
        {
            return Directory.GetFiles(_remoteStore, $"{name}.*").FirstOrDefault();
        }


    }
}
