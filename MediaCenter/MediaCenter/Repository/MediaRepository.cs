using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Media;
using MediaCenter.Media;

namespace MediaCenter.Repository
{
    public class MediaRepository
    {
        private const string ImageFileExtension = "jpg";
        

        private string _localStore;
        private readonly string _remoteStore;
        private DateTime _lastSyncFromRemote;

        // tmp
        private List<MediaItem> _mediaItems;

        public MediaRepository(string remoteStore, string localStore)
        {
            _remoteStore = remoteStore;
            _localStore = localStore;
        }

        public async Task SyncFromRemote()
        {
            // TODO: lock repository for editing

            var remoteDir = new DirectoryInfo(_remoteStore);
            foreach (var file in remoteDir.GetFiles().Where(f => f.LastWriteTime >= _lastSyncFromRemote))
            {
                string content;
                using (TextReader reader = file.OpenText())
                {
                    content = await reader.ReadToEndAsync();
                }
                if(_mediaItems.All(i => i.Name != content))
                    _mediaItems.Add(new ImageItem {Name = content});
            }
        }

        private async Task Save()
        {
            await UpdateLocalStore();
            await SyncToRemote();
        }

        public async Task SyncToRemote()
        {
            throw new NotImplementedException();
        }

        private async Task UpdateLocalStore()
        {
            throw new NotImplementedException();
        }

        
    }
}
