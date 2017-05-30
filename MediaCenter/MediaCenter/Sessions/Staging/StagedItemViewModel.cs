using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using MediaCenter.MVVM;

namespace MediaCenter.Sessions.Staging
{
    public class StagedItemViewModel : SessionItemViewModel
    {
        public StagedItem StagedItem => (StagedItem)SessionItem;
        
        public StagedItemViewModel(StagedItem stagedItem) : base(stagedItem)
        {
            
        }
        
        public string FilePath => StagedItem.FilePath;
    }
}
