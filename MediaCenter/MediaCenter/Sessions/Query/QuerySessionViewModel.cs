using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MediaCenter.MVVM;
using MediaCenter.Sessions.Query.Filters;

namespace MediaCenter.Sessions.Query
{
    public class QuerySessionViewModel : SessionViewModelBase
    {
        private MediaInfoViewModel _currentItemInfo;
        private ObservableCollection<string> _availableTags;

        public QuerySessionViewModel(SessionBase session) : base(session)
        {
            InitialzeFilterNames();
            InitializeAllTags();
        }

        public override string Name => "Query";

        public QuerySession QuerySession => (QuerySession) Session;


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
            FilterNames = new List<string> { DatePeriodFilter.Name, DayFilter.Name, TagFilter.Name, FavoriteFilter.Name };
        }
        #endregion

        #region Result items
        private QueryResultItem _selectedItem;
        public QueryResultItem SelectedItem
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
            if(CurrentItemInfo != null)
                await QuerySession.SaveItem(CurrentItemInfo.MediaInfo);
        }
        private void InitializeCurrentItem()
        {
            CurrentItemInfo = new MediaInfoViewModel(SelectedItem.Info);
            AvailableTags = new ObservableCollection<string>(AllTags.Where(t => !CurrentItemInfo.Tags.Contains(t)));
        }
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
            if(SelectedFilterName == DatePeriodFilter.Name)
                Filters.Add(new DatePeriodFilter());
            else if(SelectedFilterName == DayFilter.Name)
                Filters.Add(new DayFilter());
            else if(SelectedFilterName == TagFilter.Name)
                Filters.Add(new TagFilter());
            else if(SelectedFilterName == FavoriteFilter.Name)
                Filters.Add(new FavoriteFilter());

        }
        #endregion

        #region Command: Execute query
        private AsyncRelayCommand _executeQueryCommand;
        

        public AsyncRelayCommand ExecuteQueryCommand => _executeQueryCommand ?? (_executeQueryCommand = new AsyncRelayCommand(ExecuteQuery));
        private async Task ExecuteQuery()
        {
            await QuerySession.ExecuteQuery();
        }
        #endregion
    }
}
