using System.Drawing;
using MediaCenter.MVVM;

namespace MediaCenter.Sessions.Staging
{
    public class StagedItem : SessionItem
    {
        private string _originalFilePath;
        public string FilePath
        {
            get { return _originalFilePath; }
            set { SetValue(ref _originalFilePath, value); }
        }



    }
}
