using MediaCenter.Media;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace MediaCenter.Sessions.Query
{
    public class QueryResultListViewModel : QueryResultViewModel
    {
        public QueryResultListViewModel(ObservableCollection<MediaItem> items, MediaItem selectedItem = null) : base(null)
        {
            if(selectedItem != null && Items.Contains(selectedItem))
                SelectedItems.Add(selectedItem);
            SelectedItems.CollectionChanged += SelectedItemsOnCollectionChanged;
        }

        protected void SelectedItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            //RaiseSelectionChanged(args.OldItems.Cast<MediaItem>().ToList(), args.NewItems.Cast<MediaItem>().ToList());
        }
    }
}
