using MediaCenter.Media;
using MediaCenter.MVVM;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MediaCenter.Sessions.Query
{
    public abstract class QueryResultViewModel : PropertyChangedNotifier
    {
        public QueryResultViewModel(ObservableCollection<MediaItem> queryResultItems)
        {
            QueryResultItems = queryResultItems;
            SelectedItems = new BatchObservableCollection<MediaItem>();
        }

        public ObservableCollection<MediaItem> QueryResultItems { get; }

        public BatchObservableCollection<MediaItem> SelectedItems { get; }

        public event SelectionChangedEventHandler SelectionChanged;

        protected void RaiseSelectionChanged(List<MediaItem> itemsRemoved, List<MediaItem> itemsAdded)
        {
            SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(itemsRemoved, itemsAdded));
        }
    }
}
