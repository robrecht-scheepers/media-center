﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;
using MediaCenter.Helpers;

namespace MediaCenter.Repository
{
    public class RemoteRepository
    {
        // TODO: add "ready-only" mode to avoid concurrent editing
        private const string MediaFileExtension = ".mcd";

        private readonly string _localStoreFilePath;
        private readonly string _remoteStore;
        private DateTime _lastSyncFromRemote;
        
        private List<MediaInfo> _catalog;

        public RemoteRepository(string remoteStore, string localStoreFilePath)
        {
            _remoteStore = remoteStore;
            _localStoreFilePath = localStoreFilePath;
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
            await IOHelper.SaveObject(_catalog, _localStoreFilePath);
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
                var existingItem = _catalog.FirstOrDefault(x => x.Name == item.Name);
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
            var deleteList = _catalog.Where(item => 
                remoteStoreMediaFiles.All(f => f.Name.ToLower() != item.Name + MediaFileExtension)).ToList();
            foreach (var mediaItem in deleteList)
            {
                _catalog.Remove(mediaItem);
            }

            await UpdateLocalStore();
            _lastSyncFromRemote = newLastSyncedDate;
        }

        public async Task AddMediaItem(string filePath, string name, Image thumbnail)
        {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(name))
                return;
            
            // add to remote store
            var mediaItemFilename = Path.Combine(_remoteStore, name + Path.GetExtension(filePath));
            await IOHelper.CopyFile(filePath, mediaItemFilename);

            var thumbnailFilename = Path.Combine(_remoteStore, name + "_T.jpg");
            await IOHelper.SaveImage(thumbnail, thumbnailFilename, ImageFormat.Jpeg);

            var descriptorFilename = Path.Combine(_remoteStore, name + MediaFileExtension);
            await IOHelper.SaveObject(new MediaInfo(name), descriptorFilename);

            await SynchronizeFromRemoteStore();
            // yes, this causes a retrieval of an object we already have in memory, 
            // but it fixes the concurrent access issues by always having the remote as master
            // so it's worth it, as media files are small enough

            // TODO: optimize to do only one sync from remote when all items of a staging session have been saved
        }

         
    }
}
