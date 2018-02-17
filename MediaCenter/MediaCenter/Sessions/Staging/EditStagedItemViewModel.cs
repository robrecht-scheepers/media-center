using System;
using System.Collections.Generic;
using System.Linq;
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

        public EditStagedItemViewModel(List<StagedItem> items)
        {
            Items = items;
            DisplayItem = items.FirstOrDefault();
            if (items.Count == 1)
            {
                Title = DisplayItem.FilePath;
                OriginalDate = $"Extracted date: {DisplayItem.DateTaken:dd.MM.yyyy HH:mm:ss}";
            }
            else
            {
                Title = $"Editing {items.Count} items.";
                OriginalDate = "Extracted date: -";
            }

            NewDateTaken = DisplayItem.DateTaken;
        }

        public StagedItem DisplayItem { get; }
        public List<StagedItem> Items { get; }
        public string Title { get; }
        public string OriginalDate { get; }

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
