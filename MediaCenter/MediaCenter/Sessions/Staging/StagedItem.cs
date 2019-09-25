using MediaCenter.Media;

namespace MediaCenter.Sessions.Staging
{
    public class StagedItem : MediaItem
    {
        private string _filePath;
        
        public StagedItem(MediaType type) : base(type)
        { }

        public string FilePath
        {
            get => _filePath;
            set => SetValue(ref _filePath, value);
        }
    }
}
