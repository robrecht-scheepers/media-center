using System.Collections.Generic;
using System.Collections.ObjectModel;
using MediaCenter.MVVM;

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
            _filterNames = new List<string>();
            _filterNames.Add(DatePeriodFilter.Name);
            _filterNames.Add(DayFilter.Name);
        }

        protected override string CreateNameForSession(SessionBase session)
        {
            return "Query 1";
        }

        public QuerySession QuerySession => (QuerySession) Session;

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

        public RelayCommand AddFilterCommand
        {
            get { return _addFilterCommand ?? (_addFilterCommand = new RelayCommand(AddFilter)); }
        }

        private void AddFilter()
        {
            if(SelectedFilterName == DatePeriodFilter.Name)
                Filters.Add(new DatePeriodFilter());
            else if(SelectedFilterName == DayFilter.Name)
                Filters.Add(new DayFilter());

        }
    }
}
