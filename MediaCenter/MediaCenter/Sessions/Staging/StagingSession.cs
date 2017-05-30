using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.Helpers;
using MediaCenter.Repository;

namespace MediaCenter.Sessions.Staging
{
    public class StagingSession : SessionBase
    {
        // TODO: share with view model for dialog filter
        private string[] _imageExtensions = {".jpg", ".png", ".bmp"};
        private string _statusMessage;

        public StagingSession(RemoteRepository repository) : base(repository)
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

                //using (FileStream fileStream = File.OpenRead(filePath))
                //{
                //    byte[] buffer = new byte[fileStream.Length];
                //    int numBytesToRead = buffer.Length;
                //    int numBytesRead = 0;
                //    while (numBytesToRead > 0)
                //    {
                //        int n = await fileStream.ReadAsync(buffer, numBytesRead, numBytesToRead);
                //        if (n == 0)
                //            break;
                //        numBytesRead += n;
                //        numBytesToRead -= n;
                //    }

                //    using (Stream destination = new MemoryStream(buffer))
                //    {
                //        var image = new Bitmap(destination);
                //        var date = ReadImageDate(image);
                //        var name = CreateItemName(date);
                //        var thumbnail = await CreateThumbnail(image);
                //        StagedItems.Add(new StagedItem { Info = new MediaInfo(name) {DateTaken = date}, FilePath = filePath, Thumbnail = thumbnail});
                //    }
                //}
                var image =  await IOHelper.OpenImage(filePath);
                var date = ReadImageDate(image);
                var name = CreateItemName(date);
                var thumbnail = await CreateThumbnail(image);
                StagedItems.Add(new StagedItem { Info = new MediaInfo(name) { DateTaken = date }, FilePath = filePath, Thumbnail = thumbnail });
            }
        }

        public async Task SaveToRepository()
        {
            await Repository.SaveStagedItems(StagedItems);
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            set { SetValue(ref _statusMessage, value); }
        }

        private DateTime ReadImageDate(Image image)
        {
            int datePropertyID = 36867;
            ASCIIEncoding encoding = new ASCIIEncoding();
            string dateString = "";

            PropertyItem[] propertyItems = image.PropertyItems;
            var dateProperty = propertyItems.FirstOrDefault(p => p.Id == datePropertyID);
            if (dateProperty == null)
            {
                return DateTime.MinValue;
            }
            
            dateString = encoding.GetString(dateProperty.Value);
            dateString = dateString.Substring(0, dateString.Length - 1); // drop zero character /0
            var date = DateTime.ParseExact(dateString, "yyyy:MM:dd HH:mm:ss", new DateTimeFormatInfo());
            return date;
        }

        private string CreateItemName(DateTime date)
        {
            var name = date.ToString("yyyyMMddHHmmss");
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
