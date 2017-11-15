﻿using MediaCenter.Media;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace MediaCenter.Sessions.Query
{
    public class QueryResultListViewModel : QueryResultViewModel
    {
        public QueryResultListViewModel(ObservableCollection<MediaItem> queryResultItems) : base(queryResultItems)
        {
            SelectedItems.CollectionChanged += SelectedItemsOnCollectionChanged;
        }

        protected void SelectedItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            RaiseSelectionChanged(args.OldItems.Cast<MediaItem>().ToList(), args.NewItems.Cast<MediaItem>().ToList());
        }
    }
}
