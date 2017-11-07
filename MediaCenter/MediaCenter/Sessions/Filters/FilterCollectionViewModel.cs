using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MediaCenter.MVVM;

namespace MediaCenter.Sessions.Filters
{
    public class FilterCollectionViewModel : PropertyChangedNotifier
    {
        private IEnumerable<string> _tags;
        private List<Filter> _filters;

        public FilterCollectionViewModel(List<Filter> filters, IEnumerable<string> tags)
        {
            _tags = tags;
            _filters = filters;
            InitializeFilterViewModels();
        }

        public ObservableCollection<FilterViewModel> FilterViewModels { get; private set;}
        private void InitializeFilterViewModels()
        {
            FilterViewModels = new ObservableCollection<FilterViewModel>();
            foreach (var filter in _filters)
            {
                var filterViewModel = new FilterViewModel(filter, _tags);
                filterViewModel.FilterChanged += FilterChanged;
                FilterViewModels.Add(new FilterViewModel(filter,_tags));
            }
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            _filters.Clear();
            foreach (var filterViewModel in FilterViewModels.Where(x => x.Filter != null))
            {
                _filters.Add(filterViewModel.Filter);
            }
        }

        #region Command: Add filter
        private RelayCommand _addFilterCommand;
        public RelayCommand AddFilterCommand => _addFilterCommand ?? (_addFilterCommand = new RelayCommand(AddFilter));
        private void AddFilter()
        {
            var filterViewModel = new FilterViewModel(_tags);
            filterViewModel.FilterChanged += FilterChanged;
            FilterViewModels.Add(filterViewModel);
        }
        #endregion

        #region Command: Remove filter
        private RelayCommand<FilterViewModel> _removeFilterCommand;
        public RelayCommand<FilterViewModel> RemoveFilterCommand => _removeFilterCommand ?? (_removeFilterCommand = new RelayCommand<FilterViewModel>(RemoveFilter));
        private void RemoveFilter(FilterViewModel filterViewModel)
        {
            if (filterViewModel.Filter != null)
                _filters.Remove(filterViewModel.Filter);

            filterViewModel.FilterChanged -= FilterChanged;
            FilterViewModels.Remove(filterViewModel);
        }
        #endregion

    }
}
