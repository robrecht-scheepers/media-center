using System.Collections.Generic;
using System.Collections.ObjectModel;
using MediaCenter.MVVM;
using MediaCenter.Repository;
using System.Linq;

namespace MediaCenter.Sessions
{
    public class TagsViewModel : PropertyChangedNotifier
    {
        private ObservableCollection<string> _availableTags;
        private List<string> _originalTags;

        public TagsViewModel(IEnumerable<string> allTags, IEnumerable<string> currentTags = null)
        {
            _originalTags = currentTags?.ToList() ?? new List<string>();

            AvailableTags = new ObservableCollection<string>(allTags.OrderBy(x => x));
            SelectedTags = new ObservableCollection<string>();
            foreach(string tag in _originalTags)
            {
                AvailableTags.Add(tag);
                AvailableTags.Remove(tag);
            }            
        }

        public bool IsDirty => _originalTags.OrderBy(x => x).SequenceEqual(SelectedTags.OrderBy(x => x));
        
        public ObservableCollection<string> SelectedTags { get; private set; }

        public ObservableCollection<string> AvailableTags { get; private set; }

        private RelayCommand<string> _addExistingTagCommand;
        public RelayCommand<string> AddExistingTagCommand => _addExistingTagCommand ?? (_addExistingTagCommand = new RelayCommand<string>(AddExistingTag));
        private void AddExistingTag(string newTag)
        {
            SelectedTags.Add(newTag);
            AvailableTags.Remove(newTag);
        }

        private RelayCommand<string> _removeTagCommand;
        public RelayCommand<string> RemoveTagCommand => _removeTagCommand ?? (_removeTagCommand = new RelayCommand<string>(RemoveTag));
        private void RemoveTag(string tag)
        {
            SelectedTags.Remove(tag);
            AvailableTags.Add(tag);
        }

        private RelayCommand _addNewTagCommand;
        public RelayCommand AddNewTagCommand => _addNewTagCommand ?? (_addNewTagCommand = new RelayCommand(AddNewTag));
        private void AddNewTag()
        {
            SelectedTags.Add(NewTag);
            NewTag = "";
        }

        private string _newTag;
        public string NewTag
        {
            get { return _newTag; }
            set { SetValue(ref _newTag, value); }
        }
    }
}
