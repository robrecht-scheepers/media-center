namespace MediaCenter.Sessions.Staging
{
    public class StagedItem : SessionItem
    {
        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set { SetValue(ref _filePath, value); }
        }
    }
}
