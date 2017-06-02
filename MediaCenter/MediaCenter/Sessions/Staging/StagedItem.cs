using System.Drawing;

namespace MediaCenter.Sessions.Staging
{
    public class StagedItem : SessionItem
    {
        private string _filePath;
        private Image _thumbnail;
        private Image _fullImage;

        public string FilePath
        {
            get { return _filePath; }
            set { SetValue(ref _filePath, value); }
        }

        public Image Thumbnail
        {
            get { return _thumbnail; }
            set { SetValue(ref _thumbnail, value); }
        }

        public Image FullImage
        {
            get { return _fullImage; }
            set { SetValue(ref _fullImage, value); }
        }

    }
}
