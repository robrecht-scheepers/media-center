using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using MediaCenter.MVVM;
using MediaCenter.Sessions.Query.Filters;
using MediaCenter.Sessions.Staging;

namespace MediaCenter.Sessions.Query
{
    public class QuerySessionViewModel : SessionViewModelBase
    {
        public QuerySessionViewModel(SessionBase session) : base(session)
        {
            InitialzeFilterNames();
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
        
        public ObservableCollection<Filter> Filters => QuerySession.Filters;
        
        public List<string> FilterNames { get; private set; }

        private string _selectedFilterName;
        public string SelectedFilterName
        {
            get { return _selectedFilterName; }
            set { SetValue(ref _selectedFilterName, value);}
        }

        private QueryResultItem _selectedItem;
        public QueryResultItem SelectedItem
        {
            get { return _selectedItem; }
            set { SetValue(ref _selectedItem, value, async () => await SelectedItemChanged()); }
        }

        private async Task SelectedItemChanged()
        {
            await QuerySession.FullImageRequested(SelectedItem.Name);
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
        
        public AsyncRelayCommand ExecuteQueryCommand => _executeQueryCommand ?? (_executeQueryCommand = new AsyncRelayCommand(ExecuteQuery));
        private async Task ExecuteQuery()
        {
            await QuerySession.ExecuteQuery();
        }
        #endregion
    }
}
