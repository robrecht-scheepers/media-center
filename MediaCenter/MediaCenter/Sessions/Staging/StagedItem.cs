using MediaCenter.Repository;

namespace MediaCenter.Sessions.Staging
{
    public class StagedItem : MediaItem
    {
        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set { SetValue(ref _filePath, value); }
        }

        public StagedItem(string name, MediaType type) : base(name, type)
        {
        }
    }
}
