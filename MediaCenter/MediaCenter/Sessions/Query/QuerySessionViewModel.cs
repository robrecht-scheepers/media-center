using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MediaCenter.MVVM;
using MediaCenter.Repository;
using MediaCenter.Sessions.Query.Filters;

namespace MediaCenter.Sessions.Query
{
    public class QuerySessionViewModel : SessionViewModelBase
    {
        private MediaInfoViewModel _currentItemInfo;
        private ObservableCollection<string> _availableTags;
        private string _currentItemName; // tmp, until we get PropertyChanging

        public QuerySessionViewModel(SessionBase session) : base(session)
        {
            InitialzeFilterNames();
            InitializeAllTags();
        }

        public override string Name => "Query";

        public QuerySession QuerySession => (QuerySession) Session;

        public ObservableCollection<MediaItem> QueryResult => QuerySession.QueryResult; 

        #region Filters
        public ObservableCollection<Filter> Filters => QuerySession.Filters;
        
        public List<string> FilterNames { get; private set; }

        private string _selectedFilterName;
        public string SelectedFilterName
        {
            get { return _selectedFilterName; }
            set { SetValue(ref _selectedFilterName, value);}
        }
        private void InitialzeFilterNames()
        {
            FilterNames = new List<string> { DateTakenFilter.Name, TagFilter.Name, FavoriteFilter.Name, DateAddedFilter.Name };
        }
        #endregion

        #region Result items
        private MediaItem _selectedItem;
        public MediaItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                SetValue(ref _selectedItem, value, async () => await SelectedItemChanged());
            }
        }
        public MediaInfoViewModel CurrentItemInfo
        {
            get { return _currentItemInfo; }
            set { SetValue(ref _currentItemInfo, value); }
        }
        private async Task SelectedItemChanged()
        {
            Task imageTask = null;
            if(SelectedItem != null)
                imageTask = QuerySession.FullImageRequested(SelectedItem.Name);
            await SaveCurrentItem();
            InitializeCurrentItem();
            if(imageTask != null)
                await imageTask;
        }
        private async Task SaveCurrentItem()
        {
            if(!string.IsNullOrEmpty(_currentItemName))
                await QuerySession.SaveItem(_currentItemName); // TODO: tmp logic until we have PropertyChanging
            _currentItemName = SelectedItem.Name;

        }
        private void InitializeCurrentItem()
        {
            if (SelectedItem == null)
            {
                CurrentItemInfo = null;
                AvailableTags = new ObservableCollection<string>();
            }
            else
            {
                CurrentItemInfo = new MediaInfoViewModel(SelectedItem.Info);
                AvailableTags = new ObservableCollection<string>(AllTags.Where(t => !CurrentItemInfo.Tags.Contains(t)));
            }
        }

        #region Command: Select next image
        private RelayCommand _selectNextImageCommand;
        public RelayCommand SelectNextImageCommand
            => _selectNextImageCommand ?? (_selectNextImageCommand = new RelayCommand(SelectNextImage, CanExecuteSelectNextImage));
        private void SelectNextImage()
        {
            SelectedItem = QueryResult[QueryResult.IndexOf(SelectedItem) + 1];
        }
        private bool CanExecuteSelectNextImage()
        {
            if (SelectedItem == null)
                return false;
            return QueryResult.IndexOf(SelectedItem) < QueryResult.Count - 1;
        }
        #endregion

        #region Command: Select previous image
        private RelayCommand _selectPreviousImageCommand;
        public RelayCommand SelectPreviousImageCommand
            => _selectPreviousImageCommand ?? (_selectPreviousImageCommand = new RelayCommand(SelectPreviousImage, CanExecuteSelectPreviousImage));
        private void SelectPreviousImage()
        {
            SelectedItem = QueryResult[QueryResult.IndexOf(SelectedItem) - 1];
        }
        private bool CanExecuteSelectPreviousImage()
        {
            if (SelectedItem == null)
                return false;
            return QueryResult.IndexOf(SelectedItem) > 0;
        }
        #endregion


        #endregion

        #region Tags

        public ObservableCollection<string> AllTags { get; private set; }
        private void InitializeAllTags()
        {
            AllTags = new ObservableCollection<string>();
            foreach (var tag in QuerySession.Repository.Tags.OrderBy(s => s))
            {
                AllTags.Add(tag);
            }
        }

        public ObservableCollection<string> AvailableTags
        {
            get { return _availableTags; }
            set { SetValue(ref _availableTags, value); }
        }

        private RelayCommand<string> _addTagCommand;
        public RelayCommand<string> AddTagCommand => _addTagCommand ?? (_addTagCommand = new RelayCommand<string>(AddTag));
        private void AddTag(string newTag)
        {
            CurrentItemInfo.Tags.Add(newTag);
            AvailableTags.Remove(newTag);
        }

        private RelayCommand<string> _removeTagCommand;
        public RelayCommand<string> RemoveTagCommand => _removeTagCommand ?? (_removeTagCommand = new RelayCommand<string>(RemoveTag));
        private void RemoveTag(string tag)
        {
            CurrentItemInfo.Tags.Remove(tag);
            AvailableTags.Add(tag);
        }

        private RelayCommand _addNewTagCommand;
        public RelayCommand AddNewTagCommand => _addNewTagCommand ?? (_addNewTagCommand = new RelayCommand(AddNewTag));
        private void AddNewTag()
        {
            CurrentItemInfo.Tags.Add(NewTag);
            AllTags.Add(NewTag);
            NewTag = "";
        }

        private string _newTag;
        public string NewTag
        {
            get { return _newTag; }
            set { SetValue(ref _newTag,value); }
        }

        #endregion

        #region Command: Add filter
        private RelayCommand _addFilterCommand;
        public RelayCommand AddFilterCommand => _addFilterCommand ?? (_addFilterCommand = new RelayCommand(AddFilter));
        private void AddFilter()
        {
            if(SelectedFilterName == DateTakenFilter.Name)
                Filters.Add(new DateTakenFilter());
            else if(SelectedFilterName == TagFilter.Name)
                Filters.Add(new TagFilter());
            else if(SelectedFilterName == FavoriteFilter.Name)
                Filters.Add(new FavoriteFilter());
            else if(SelectedFilterName == DateTakenFilter.Name)
                Filters.Add(new DateAddedFilter());

        }
        #endregion

        #region Command: Remove filter
        private RelayCommand<Filter> _removeFilterCommand;
        public RelayCommand<Filter> RemoveFilterCommand => _removeFilterCommand ?? (_removeFilterCommand = new RelayCommand<Filter>(RemoveFilter));
        private void RemoveFilter(Filter filter)
        {
            Filters.Remove(filter);
        }
        #endregion

        #region Command: Execute query
        private AsyncRelayCommand _executeQueryCommand;
        public AsyncRelayCommand ExecuteQueryCommand => _executeQueryCommand ?? (_executeQueryCommand = new AsyncRelayCommand(ExecuteQuery));
        private async Task ExecuteQuery()
        {
            await QuerySession.ExecuteQuery();
            SelectedItem = QueryResult.FirstOrDefault();
        }
        #endregion
    }
}
