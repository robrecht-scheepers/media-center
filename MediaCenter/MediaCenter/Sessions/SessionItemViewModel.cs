using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace MediaCenter.Sessions
{
    public class SessionItemViewModel
    {
        protected readonly SessionItem SessionItem;

        public SessionItemViewModel(SessionItem sessionItem)
        {
            SessionItem = sessionItem;
            Thumbnail = ImageToBitmapImage(SessionItem.Thumbnail);
        }

        private BitmapImage ImageToBitmapImage(Image source)
        {
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
        
        public BitmapImage Thumbnail { get; }
    }
}
