using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using MediaCenter.MVVM;

namespace MediaCenter.Sessions
{
    public class SessionItemViewModel : Observable
    {
        protected readonly SessionItem SessionItem;
        private BitmapImage _thumbnail;

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
        }

        private void SessionItem_ThumbnailUpdated()
        {
            Thumbnail = ImageToBitmapImage(SessionItem.Thumbnail);
        }

        private BitmapImage ImageToBitmapImage(Image source)
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
            }
            return result;
        }

        public string Name => SessionItem.Name;

        public BitmapImage Thumbnail
        {
            get { return _thumbnail; }
            set { SetValue(ref _thumbnail, value); }
        }

        
    }
}
