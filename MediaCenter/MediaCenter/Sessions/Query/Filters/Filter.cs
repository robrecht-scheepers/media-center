using System.Collections.Generic;
using MediaCenter.MVVM;
using MediaCenter.Repository;

namespace MediaCenter.Sessions.Query.Filters
{
    public abstract class Filter : PropertyChangedNotifier
    {
        protected Filter()
        {
            FilterMode = FilterMode.Match;
        }

        public FilterMode FilterMode { get; set; }

        public abstract IEnumerable<MediaItem> Apply(IEnumerable<MediaItem> source);
    }
}
