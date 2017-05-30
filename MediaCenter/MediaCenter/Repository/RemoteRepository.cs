﻿using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
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
        
        public IEnumerable<MediaInfo> Catalog => _catalog;

        public RemoteRepository(string remoteStore, string localStoreFilePath, string localCachePath)
        {
            _remoteStore = remoteStore;
            _localStoreFilePath = localStoreFilePath;
            _localCachePath = localCachePath;
            _catalog = new List<MediaInfo>();
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
                await IOHelper.SaveImage(newItem.Thumbnail, thumbnailFilename, ImageFormat.Jpeg);

                var descriptorFilename = Path.Combine(_remoteStore, newItem.Name + MediaFileExtension);
                await IOHelper.SaveObject(newItem.Info, descriptorFilename);
            }

            await SynchronizeFromRemoteStore();
            // yes, this causes a retrieval of an object we already have in memory, 
            // but it fixes the concurrent access issues by always having the remote as master
            // so it's worth it, as media files are small enough
        }

        public async Task<Image> GetThumbnail(string name)
        {
            var thumbnailFilename = Path.Combine(_remoteStore, name + "_T.jpg");
            return await IOHelper.OpenImage(thumbnailFilename);
        }

        public async Task<Image> GetImage(string name)
        {
            var imagePath = Directory.GetFiles(_remoteStore, $"{name}.*").FirstOrDefault();
            if (string.IsNullOrEmpty(imagePath))
                return null;
            
            // check if image is present in cache, if not, copy to cache
            var imageCachePath = Path.Combine(_localCachePath, Path.GetFileName(imagePath));
            if(!File.Exists(imageCachePath))
                await IOHelper.CopyFile(imagePath, imageCachePath);
            // TODO: cache cleanup

            // open image from cache
            return await IOHelper.OpenImage(imageCachePath);
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
    }
}
