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
        private readonly List<Filter> _filters;

        public FilterCollectionViewModel(IEnumerable<string> tags)
        {
            _tags = tags;
            _filters = new List<Filter>();
            InitializeFilterViewModels();
        }

        public ObservableCollection<FilterViewModel> FilterViewModels { get; private set;}
        private void InitializeFilterViewModels()
        {
            FilterViewModels = new ObservableCollection<FilterViewModel>();
            foreach (var filter in _filters)
            {
                var filterViewModel = new FilterViewModel(filter, _tags);
                filterViewModel.FilterChanged += OnFilterChanged;
                FilterViewModels.Add(new FilterViewModel(filter,_tags));
            }
            InitializeEmptyFilter();
        }

        private void OnFilterChanged(object sender, EventArgs e)
        {
            UpdateFilters();
            InitializeEmptyFilter();
            RaiseFilterChanged();
        }

        private void UpdateFilters()
        {
            _filters.Clear();
            foreach (var filterViewModel in FilterViewModels.Where(x => x.Filter != null))
            {
                _filters.Add(filterViewModel.Filter);
            }
        }

        #region Command: Remove filter
        private RelayCommand<FilterViewModel> _removeFilterCommand;
        public RelayCommand<FilterViewModel> RemoveFilterCommand => _removeFilterCommand ?? (_removeFilterCommand = new RelayCommand<FilterViewModel>(RemoveFilter));
        private void RemoveFilter(FilterViewModel filterViewModel)
        {
            if (filterViewModel.Filter != null)
                _filters.Remove(filterViewModel.Filter);

            filterViewModel.FilterChanged -= OnFilterChanged;
            FilterViewModels.Remove(filterViewModel);

            RaiseFilterChanged();
        }
        #endregion

        private void InitializeEmptyFilter()
        {
            // make sure the list end with an empty filter, that allows the user to creat the next one
            if (!FilterViewModels.Any() || FilterViewModels.Last().Filter != null)
            {
                var filterViewModel = new FilterViewModel(_tags);
                filterViewModel.FilterChanged += OnFilterChanged;
                FilterViewModels.Add(filterViewModel);
            }
        }

        public event EventHandler FilterChanged;
        private void RaiseFilterChanged()
        {
            FilterChanged?.Invoke(this, EventArgs.Empty);
        }

    }
}
