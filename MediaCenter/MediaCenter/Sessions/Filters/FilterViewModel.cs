using System;
using System.Collections.Generic;
using System.Linq;
using MediaCenter.MVVM;

namespace MediaCenter.Sessions.Filters
{
    public class FilterViewModel : PropertyChangedNotifier
    {
        private Filter _filter;
        private string _selectedFilterName;
        private List<string> _tags;

        public FilterViewModel(IEnumerable<string> tags)
        {
            _tags = tags.ToList();
            InitialzeFilterNames();
            SelectedFilterName = "";
        }

        public FilterViewModel(Filter filter, IEnumerable<string> tags) : this(tags)
        {
            // set private members instead of properties to avoid PropertyChanged being raised during intialization
            _filter = filter;
            _filter.PropertyChanged += Filter_PropertyChanged;
            _selectedFilterName = Filter.GetName(filter); 
        }

        private void Filter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaiseFilterChanged();
        }

        public event EventHandler FilterChanged;
        private void RaiseFilterChanged()
        {
            FilterChanged?.Invoke(this,EventArgs.Empty);
        }

        public Filter Filter
        {
            get { return _filter; }
            set { SetValue(ref _filter, value, RaiseFilterChanged); }
        }

        public List<string> FilterNames { get; private set; }
        private void InitialzeFilterNames()
        {
            FilterNames = new List<string> { DateTakenFilter.Name, TagFilter.Name, FavoriteFilter.Name, DateAddedFilter.Name, PrivateFilter.Name, MediaTypeFilter.Name };
        }
        public string SelectedFilterName
        {
            get { return _selectedFilterName; }
            set { SetValue(ref _selectedFilterName, value, SelectedFilterNameChanged); }
        }

        private void SelectedFilterNameChanged()
        {
            if (Filter != null)
                Filter.PropertyChanged -= Filter_PropertyChanged;

            if (string.IsNullOrEmpty(SelectedFilterName))
            {
                Filter = null;
            }
            else
            {
                Filter = Filter.Create(SelectedFilterName, _tags);
                Filter.PropertyChanged += Filter_PropertyChanged;
            }
        }
        
    }
}
