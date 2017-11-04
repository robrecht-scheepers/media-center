using MediaCenter.Media;
using MediaCenter.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCenter.Sessions.Query
{
    public abstract class QueryResultViewModel : PropertyChangedNotifier
    {
        public QueryResultViewModel(ObservableCollection<MediaItem> queryResultItems)
        {
            QueryResultItems = queryResultItems;
            SelectedItems = new ObservableCollection<MediaItem>();
        }

        public ObservableCollection<MediaItem> QueryResultItems { get; }

        public ObservableCollection<MediaItem> SelectedItems { get; }

        public event SelectionChangedEventHandler SelectionChanged;

        protected void RaiseSelectionChanged(List<MediaItem> oldItems, List<MediaItem> newItems)
        {
            SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(oldItems, newItems));
        }
    }
}
