using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using MediaCenter.MVVM;
using MediaCenter.Sessions.Staging;

namespace MediaCenter.Sessions.Query
{
    public class QuerySessionViewModel : SessionViewModelBase
    {
        public QuerySessionViewModel(SessionBase session) : base(session)
        {
            InitialzeFilterNames();
            QueryResultItems = new ObservableCollection<SessionItemViewModel>();
            QuerySession.QueryResult.CollectionChanged += QueryResult_CollectionChanged;
        }

        private void QueryResult_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Reset)
                QueryResultItems.Clear();

            if (e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems)
                {
                    QueryResultItems.Remove(QueryResultItems.First(x => x.Name == ((StagedItem)oldItem).Name));
                }
            }
            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems)
                {
                    QueryResultItems.Add(new SessionItemViewModel((SessionItem)newItem));
                }
            }
        }

        private void InitialzeFilterNames()
        {
            FilterNames = new List<string> {DatePeriodFilter.Name, DayFilter.Name};
        }

        protected override string CreateNameForSession(SessionBase session)
        {
            return "Query 1";
        }

        public QuerySession QuerySession => (QuerySession) Session;

        public ObservableCollection<SessionItemViewModel> QueryResultItems { get; }

        public ObservableCollection<Filter> Filters => QuerySession.Filters;

        private string _selectedFilterName;

        public List<string> FilterNames { get; private set; }

        public string SelectedFilterName
        {
            get { return _selectedFilterName; }
            set { SetValue(ref _selectedFilterName, value);}
        }

        public SessionItemViewModel SelectedItem
        {
            get { return _selectedItem; }
            set { SetValue(ref _selectedItem, value, async () => await SelectedItemChanged()); }
        }

        private async Task SelectedItemChanged()
        {
            await QuerySession.LoadImageForSessionItem(SelectedItem.Name);
        }

        #region Add filter
        private RelayCommand _addFilterCommand;
        public RelayCommand AddFilterCommand => _addFilterCommand ?? (_addFilterCommand = new RelayCommand(AddFilter));
        private void AddFilter()
        {
            if(SelectedFilterName == DatePeriodFilter.Name)
                Filters.Add(new DatePeriodFilter());
            else if(SelectedFilterName == DayFilter.Name)
                Filters.Add(new DayFilter());

        }
        #endregion

        #region Execute query
        private AsyncRelayCommand _executeQueryCommand;
        private SessionItemViewModel _selectedItem;
        public AsyncRelayCommand ExecuteQueryCommand => _executeQueryCommand ?? (_executeQueryCommand = new AsyncRelayCommand(ExecuteQuery));
        private async Task ExecuteQuery()
        {
            await QuerySession.ExecuteQuery();
        }
        #endregion
    }
}
