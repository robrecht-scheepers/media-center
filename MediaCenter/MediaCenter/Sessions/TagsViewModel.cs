using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.MVVM;
using MediaCenter.Repository;

namespace MediaCenter.Sessions
{
    public class TagsViewModel : PropertyChangedNotifier
    {
        private ObservableCollection<string> _availableTags;
        private IRepository _repository;
        private List<string> _originalTags;

        public TagsViewModel(IRepository repository, List<string> currentTags = null)
        {
            _repository = repository;
            _originalTags = currentTags ?? new List<string>();

            SelectedTags = new ObservableCollection<string>(_originalTags);
        }

        public bool IsDirty
        {
            get
            {
                return false;// TODO: compare two lists
            }
        }

        public ObservableCollection<string> AllTags { get; private set; }
        private void InitializeAllTags()
        {
            AllTags = new ObservableCollection<string>(_repository.Tags.OrderBy(s => s));
        }
        public ObservableCollection<string> SelectedTags { get; private set; }

        public ObservableCollection<string> AvailableTags
        {
            get { return _availableTags; }
            set { SetValue(ref _availableTags, value); }
        }

        private RelayCommand<string> _addTagCommand;
        public RelayCommand<string> AddTagCommand => _addTagCommand ?? (_addTagCommand = new RelayCommand<string>(AddTag));
        private void AddTag(string newTag)
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
            AllTags.Add(NewTag);
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
