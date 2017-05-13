using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.Media;

namespace MediaCenter.Repository
{
    class Repository
    {
        
        private string _localStore;
        private string _remoteStore;
        private DateTime _lastSyncFromRemote;

        // tmp
        private List<MediaItem> _mediaItems; 

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
                    _mediaItems.Add(new Image {Name = content});
            }
        }

        
        public void AddMediaItems(IEnumerable<MediaItem> newItems)
        {
            foreach (var newItem in newItems)
            {
                if(_mediaItems.Any(i => i.Name == newItem.Name))
                    continue;
                _mediaItems.Add(newItem);
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
