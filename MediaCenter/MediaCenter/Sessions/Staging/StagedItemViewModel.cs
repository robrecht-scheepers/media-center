using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using MediaCenter.MVVM;

namespace MediaCenter.Sessions.Staging
{
    public class StagedItemViewModel : Observable
    {
        public StagedItem StagedItem { get; }
        
        public StagedItemViewModel(StagedItem stagedItem)
        {
            StagedItem = stagedItem;
            StagedItem.PropertyChanged += SessionItem_PropertyChanged;
            Thumbnail = ImageToBitmapImage(StagedItem.Thumbnail);
        }

        private void SessionItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Thumbnail")
                SessionItem_ThumbnailUpdated();
        }

        private void SessionItem_ThumbnailUpdated()
        {
            Thumbnail = ImageToBitmapImage(StagedItem.Thumbnail);
        }

        private BitmapImage _thumbnail;
        public BitmapImage Thumbnail
        {
            get { return _thumbnail; }
            set { SetValue(ref _thumbnail, value); }
        }

        public string FilePath => StagedItem.FilePath;

        private BitmapImage ImageToBitmapImage(Image source)
        {
            try
            {
                if (source == null)
                    return null;

                BitmapImage result;
                using (var ms = new MemoryStream())
                {
                    source.Save(ms, ImageFormat.Jpeg);
                    ms.Position = 0;

                    result = new BitmapImage();
                    result.BeginInit();
                    result.CacheOption = BitmapCacheOption.OnLoad;
                    result.StreamSource = ms;
                    result.EndInit();
                    result.Freeze();
                }
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
