using MediaCenter.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCenter.Sessions.Query.Filters
{
    public class FilterCollectionViewModel : PropertyChangedNotifier
    {
        public FilterCollectionViewModel(ObservableCollection<Filter> filters)
        {
            Filters = filters;
            InitialzeFilterNames();
        }

        public ObservableCollection<Filter> Filters { get; }

        public List<string> FilterNames { get; private set; }

        private string _selectedFilterName;
        public string SelectedFilterName
        {
            get { return _selectedFilterName; }
            set { SetValue(ref _selectedFilterName, value); }
        }
        private void InitialzeFilterNames()
        {
            FilterNames = new List<string> { DateTakenFilter.Name, TagFilter.Name, FavoriteFilter.Name, DateAddedFilter.Name, PrivateFilter.Name, MediaTypeFilter.Name };
        }

        #region Command: Add filter
        private RelayCommand _addFilterCommand;
        public RelayCommand AddFilterCommand => _addFilterCommand ?? (_addFilterCommand = new RelayCommand(AddFilter));
        private void AddFilter()
        {
            if (SelectedFilterName == DateTakenFilter.Name)
                Filters.Add(new DateTakenFilter());
            else if (SelectedFilterName == TagFilter.Name)
                Filters.Add(new TagFilter());
            else if (SelectedFilterName == FavoriteFilter.Name)
                Filters.Add(new FavoriteFilter());
            else if (SelectedFilterName == DateAddedFilter.Name)
                Filters.Add(new DateAddedFilter());
            else if (SelectedFilterName == PrivateFilter.Name)
                Filters.Add(new PrivateFilter());
            else if (SelectedFilterName == MediaTypeFilter.Name)
                Filters.Add(new MediaTypeFilter());

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

    }
}
