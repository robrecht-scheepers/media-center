using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.Repository;

namespace MediaCenter.Sessions
{
    public class StagingSession : SessionBase
    {
        // TODO: share with view model for dialog filter
        private string[] _imageExtensions = {".jpg", ".png", ".bmp"};
        private string _statusMessage;

        public StagingSession(MediaRepository repository) : base(repository)
        {
            StagedItems = new ObservableCollection<StagedItem>();
        }

        public ObservableCollection<StagedItem> StagedItems { get; }

        public async Task AddMediaItemsFolder(string folderPath)
        {
            await AddMediaItems(Directory.GetFiles(folderPath));
        }
        public async Task AddMediaItems(IEnumerable<string> newItems)
        {
            var total = newItems.Count();
            var cnt = 1;

            foreach (var filePath in newItems)
            {
                StatusMessage = $"Loading item {cnt++} of {total}.";
                if ((string.IsNullOrEmpty(filePath))||(!_imageExtensions.Contains(Path.GetExtension(filePath).ToLower())))
                    continue;

                using (FileStream fileStream = File.OpenRead(filePath))
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
                        var name = CreateItemName(ReadImageDate(image));
                        var thumbnail = await CreateThumbnail(image);
                        StagedItems.Add(new StagedItem {FilePath = filePath,Name = name, Thumbnail = thumbnail});
                    }
                }
            }
        }

        public async Task SaveToRepository()
        {
            var total = StagedItems.Count;
            var cnt = 1;
            foreach (var stagedItem in StagedItems)
            {
                StatusMessage = $"Uploading item {cnt++} of {total}.";
                await Repository.AddMediaItem(stagedItem.FilePath, stagedItem.Name, stagedItem.Thumbnail);
            }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            set { SetValue(ref _statusMessage, value); }
        }

        private string ReadImageDate(Image image)
        {
            int datePropertyID = 36867;
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
            var name = date.Replace("\0","").Replace(":", "").Replace(" ", "");
            //TODO: guarantee name uniqueness
            //if (_mediaItems.Any(i => i.Name == name))
            //{
            //    var originalName = name;
            //    int cnt = 2;
            //    while (_mediaItems.Any(i => i.Name == name))
            //    {
            //        name = originalName + "_" + cnt;
            //    }
            //}
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
