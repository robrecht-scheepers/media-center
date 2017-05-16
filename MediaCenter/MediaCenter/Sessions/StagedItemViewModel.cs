using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MediaCenter.Sessions
{
    public class StagedItemViewModel
    {
        private readonly StagedItem _stagedItem;
        
        public StagedItemViewModel(StagedItem stagedItem)
        {
            _stagedItem = stagedItem;
            Thumbnail = ImageToBitmapImage(_stagedItem.Thumbnail);
        }

        private BitmapImage ImageToBitmapImage(Image source)
        {
            BitmapImage result;
            using (var ms = new MemoryStream())
            {
                source.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;

                result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = ms;
                result.EndInit();
            }
            return result;
        }

        public string Name => _stagedItem.Name;
        public string FilePath => _stagedItem.FilePath;
        public BitmapImage Thumbnail { get; }
    }
}
