using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MediaCenter.MVVM;

namespace MediaCenter.Sessions
{
    public class SessionItemViewModel : Observable
    {
        protected readonly SessionItem SessionItem;
        private BitmapImage _thumbnail;
        private BitmapImage _fullImage;

        public SessionItemViewModel(SessionItem sessionItem)
        {
            SessionItem = sessionItem;
            SessionItem.PropertyChanged += SessionItem_PropertyChanged;
            Thumbnail = ImageToBitmapImage(SessionItem.Thumbnail);
        }

        private void SessionItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Thumbnail")
                SessionItem_ThumbnailUpdated();
            else if (e.PropertyName == "FullImage")
                SessionItem_FullImageUpdated();

        }

        private void SessionItem_FullImageUpdated()
        {
            FullImage = ImageToBitmapImage(SessionItem.FullImage);
        }

        private void SessionItem_ThumbnailUpdated()
        {
            Thumbnail = ImageToBitmapImage(SessionItem.Thumbnail);
        }

        private BitmapImage ImageToBitmapImage(Image source)
        {
            try
            {
                if (source == null)
                    return null;

                BitmapImage result;
                using (var ms = new MemoryStream())
                {
                    source.Save(ms, ImageFormat.Png);
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

        public string Name => SessionItem.Name;

        public BitmapImage Thumbnail
        {
            get { return _thumbnail; }
            set { SetValue(ref _thumbnail, value); }
        }

        public BitmapImage FullImage
        {
            get { return _fullImage; }
            set { SetValue(ref _fullImage, value); }
        }
    }
}
