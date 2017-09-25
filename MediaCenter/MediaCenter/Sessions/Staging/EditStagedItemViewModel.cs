using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.Media;
using MediaCenter.MVVM;

namespace MediaCenter.Sessions.Staging
{
    public enum EditViewModelCloseType { Save, Abort}
    public class CloseEditViewModelEventArgs : EventArgs
    {
        public CloseEditViewModelEventArgs(EditViewModelCloseType closeType)
        {
            CloseType = closeType;
        }
        public EditViewModelCloseType CloseType { get; set; }
    }
    public class EditStagedItemViewModel : PropertyChangedNotifier
    {
        private DateTime _newDateTaken;
        private RelayCommand _saveCommand;
        private RelayCommand _abortCommand;

        public EditStagedItemViewModel(MediaItem mediaItem)
        {
            MediaItem = mediaItem;
            NewDateTaken = MediaItem.DateTaken;
        }

        public MediaItem MediaItem { get; }

        public DateTime NewDateTaken
        {
            get { return _newDateTaken; }
            set { SetValue(ref _newDateTaken, value); }
        }

        public RelayCommand SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new RelayCommand(Save)); }
        }

        public void Save()
        {
            CloseRequested?.Invoke(this, new CloseEditViewModelEventArgs(EditViewModelCloseType.Save));
        }

        public RelayCommand AbortCommand
        {
            get { return _abortCommand ?? (_abortCommand = new RelayCommand(Abort)); }
        }

        public void Abort()
        {
            CloseRequested?.Invoke(this, new CloseEditViewModelEventArgs(EditViewModelCloseType.Abort));
        }

        public delegate void CloseEventDelegate(object sender, CloseEditViewModelEventArgs args);
        public event CloseEventDelegate CloseRequested;
        
    }
}
