using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
            _filterNames = new List<string>();
            _filterNames.Add(DatePeriodFilter.Name);
            _filterNames.Add(DayFilter.Name);
        }

        protected override string CreateNameForSession(SessionBase session)
        {
            return "Query 1";
        }

        public QuerySession QuerySession => (QuerySession) Session;

        public ObservableCollection<SessionItemViewModel> QueryResultItems { get; }

        public ObservableCollection<Filter> Filters => QuerySession.Filters;

        private List<string> _filterNames;
        private string _selectedFilterName;

        public List<string> FilterNames
        {
            get { return _filterNames; }
        }

        public string SelectedFilterName
        {
            get { return _selectedFilterName; }
            set { SetValue(ref _selectedFilterName, value);}
        }

        private RelayCommand _addFilterCommand;
        public RelayCommand AddFilterCommand => _addFilterCommand ?? (_addFilterCommand = new RelayCommand(AddFilter));

        private void AddFilter()
        {
            if(SelectedFilterName == DatePeriodFilter.Name)
                Filters.Add(new DatePeriodFilter());
            else if(SelectedFilterName == DayFilter.Name)
                Filters.Add(new DayFilter());

        }

        private RelayCommand _applyFiltersCommand;
        public RelayCommand ApplyFiltersCommand => _applyFiltersCommand ?? (_applyFiltersCommand = new RelayCommand(ApplyFilters));

        private void ApplyFilters()
        {
            QuerySession.ExecuteQuery();
        }
    }
}
