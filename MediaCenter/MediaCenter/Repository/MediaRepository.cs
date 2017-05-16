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
        private const int datePropertyID = 36867;

        private string _localStore;
        private string _remoteStore;
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

        public async Task AddMediaItems(IEnumerable<FileInfo> newItems)
        {
            // TODO: var stagingList
            foreach (var fileInfo in newItems.Where(f => f.Extension == ImageFileExtension))
            {
                string name;
                Image thumbnail;

                using (FileStream fileStream = fileInfo.OpenRead())
                {
                    byte[] buffer = new byte[fileStream.Length];
                    int numBytesToRead = buffer.Length;
                    int numBytesRead = 0;
                    while (numBytesToRead > 0)
                    {
                        int n = await fileStream.ReadAsync(buffer, numBytesRead, numBytesToRead);
                        if (n == 0)
                            break;
                        numBytesRead += n;
                        numBytesToRead -= n;
                    }

                    using (Stream destination = new MemoryStream(buffer))
                    {
                        var image = new Bitmap(destination);
                        name = CreateItemName(ReadImageDate(image));
                        thumbnail = await CreateThumbnail(image);
                    }
                }

                

                // create thumbnail 

                // create repository entry

                // 
            }
        }


        public async Task AddMediaItems(IEnumerable<MediaItem> newItems)
        {
            foreach (var newItem in newItems)
            {
                if(_mediaItems.Any(i => i.Name == newItem.Name))
                    continue;
                _mediaItems.Add(newItem);
            }
            await Save();
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

        private string ReadImageDate(Image image)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            string date = "";

            PropertyItem[] propertyItems = image.PropertyItems;
            var dateProperty = propertyItems.FirstOrDefault(p => p.Id == datePropertyID);
            if (dateProperty != null)
                date = encoding.GetString(dateProperty.Value);
                
            return date;
        }

        private string CreateItemName(string date)
        {
            var name = date.Replace(":", "").Replace(" ", "");
            if (_mediaItems.Any(i => i.Name == name))
            {
                var originalName = name;
                int cnt = 2;
                while (_mediaItems.Any(i => i.Name == name))
                {
                    name = originalName + "_" + cnt;
                }
            }
            return name;
        }

        private async Task<Image> CreateThumbnail(Image source)
        {
            Image thumbnail = null;
            Image.GetThumbnailImageAbort myCallback =
                             new Image.GetThumbnailImageAbort(ThumbnailCallback);
            await Task.Run(() => { thumbnail = source.GetThumbnailImage(100, 100, myCallback, IntPtr.Zero); });
            return thumbnail;
        }

        public bool ThumbnailCallback()
        {
            return false;
        }
    }
}
