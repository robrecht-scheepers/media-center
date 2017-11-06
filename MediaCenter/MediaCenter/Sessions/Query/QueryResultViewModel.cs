using MediaCenter.Media;
using MediaCenter.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
            SelectedItems = new SmartObservableCollection<MediaItem>();
        }

        public ObservableCollection<MediaItem> QueryResultItems { get; }

        public SmartObservableCollection<MediaItem> SelectedItems { get; }

        public event SelectionChangedEventHandler SelectionChanged;

        protected void RaiseSelectionChanged(List<MediaItem> itemsRemoved, List<MediaItem> itemsAdded)
        {
            SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(itemsRemoved, itemsAdded));
        }
    }
}
