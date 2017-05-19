using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Media;
using MediaCenter.Helpers;
using MediaCenter.Media;

namespace MediaCenter.Repository
{
    public class MediaRepository
    {
        private const string ImageFileExtension = "jpg";
        

        private string _localStoreFilePath;
        private readonly string _remoteStore;
        private DateTime _lastSyncFromRemote;
        
        private MediaItemCollection _mediaItemCollection;

        public MediaRepository(string remoteStore, string localStoreFilePath)
        {
            _remoteStore = remoteStore;
            _localStoreFilePath = localStoreFilePath;
            ReadLocalStore(); // TODO: handle that constructor ends without local store read, via a state field

        }

        private async Task ReadLocalStore()
        {
            if (!File.Exists(_localStoreFilePath))
            {
                _mediaItemCollection = new MediaItemCollection();
                await UpdateLocalStore();
                return;
            }

            _mediaItemCollection = await IOHelper.OpenObject<MediaItemCollection>(_localStoreFilePath);
        }

        private async Task UpdateLocalStore()
        {
            await IOHelper.SaveObject(_mediaItemCollection, _localStoreFilePath);
        }

        //    public async Task SyncFromRemote()
        //    {
        //        TODO: lock repository for editing

        //       var remoteDir = new DirectoryInfo(_remoteStore);
        //        foreach (var file in remoteDir.GetFiles().Where(f => f.LastWriteTime >= _lastSyncFromRemote))
        //                {
        //                    string content;
        //                    using (TextReader reader = file.OpenText())
        //                    {
        //                        content = await reader.ReadToEndAsync();
        //                    }
        //                    if (_mediaItems.All(i => i.Name != content))
        //                        _mediaItems.Add(new ImageItem(content});
        //    }
        //}

        private async Task Save()
        {
            await UpdateLocalStore();
            await SyncToRemote();
        }

        public async Task SyncToRemote()
        {
            throw new NotImplementedException();
        }

        public async Task AddMediaItem(string filePath, string name, Image thumbnail)
        {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(name))
                return;

            // save in localstore
            _mediaItemCollection.Items.Add(new ImageItem(name));
            await UpdateLocalStore();
            // TODO: refactor so we save only once when processing a staging session
            
            // add to remote store
            var mediaItemFilename = Path.Combine(_remoteStore, name + Path.GetExtension(filePath));
            await IOHelper.CopyFile(filePath, mediaItemFilename);

            var thumbnailFilename = Path.Combine(_remoteStore, name + "_T.jpg");
            await IOHelper.SaveImage(thumbnail, thumbnailFilename, ImageFormat.Jpeg);

            var descriptorFilename = Path.Combine(_remoteStore, name + ".mcd");
            await IOHelper.SaveText("", descriptorFilename);

            //TODO: update local store
        }
    }
}
