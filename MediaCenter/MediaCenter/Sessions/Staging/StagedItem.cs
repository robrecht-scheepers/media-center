using System.Windows.Shapes;
using MediaCenter.Media;

namespace MediaCenter.Sessions.Staging
{
    public class StagedItem : MediaItem
    {
        private string _filePath;

        public StagedItem(MediaType type, string filePath) : base(type)
        {
            FilePath = filePath;
            ContentFileName = System.IO.Path.GetFileName(filePath);
        }

        public string FilePath
        {
            get => _filePath;
            set => SetValue(ref _filePath, value);
        }
    }
}
