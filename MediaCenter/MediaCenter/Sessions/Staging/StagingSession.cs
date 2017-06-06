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
            var newItemsList = newItems.ToList();
            var total = newItemsList.Count();
            var cnt = 1;

            foreach (var filePath in newItemsList)
            {
                StatusMessage = $"Loading item {cnt++} of {total}.";
                if ((string.IsNullOrEmpty(filePath))||(!_imageExtensions.Contains(Path.GetExtension(filePath).ToLower())))
                    continue;
                
                var image =  await IOHelper.OpenImage(filePath);
                var date = ReadImageDate(image);
                var name = CreateItemName(date);
                var thumbnail = await CreateThumbnail(image);
                image.Dispose();
                
                StagedItems.Add(new StagedItem { Info = new MediaInfo(name) { DateTaken = date }, FilePath = filePath, Thumbnail = thumbnail });
            }
        }

        public void RemoveStagedItem(StagedItem item)
        {
            if(StagedItems.Contains(item))
                StagedItems.Remove(item);
        }

        public async Task SaveToRepository()
        {
            StatusMessage = $"Saving {StagedItems.Count} items...";
            await Repository.SaveStagedItems(StagedItems);
            StatusMessage = "All items saved";
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

        private async Task<byte[]> CreateThumbnail(Image source)
        {
            Image thumbnail = null;
            Image.GetThumbnailImageAbort myCallback =
                             new Image.GetThumbnailImageAbort(ThumbnailCallback);
            await Task.Run(() => { thumbnail = source.GetThumbnailImage(100, 100, myCallback, IntPtr.Zero); });

            MemoryStream ms = new MemoryStream();
            thumbnail.Save(ms,ImageFormat.Jpeg);
            return ms.ToArray();
        }
        public bool ThumbnailCallback()
        {
            return false;
        }


    }
}
