using System.Collections.Generic;
using MediaCenter.MVVM;
using MediaCenter.Repository;

namespace MediaCenter.Sessions.Query
{
    public abstract class Filter : Observable
    {
        public FilterMode FilterMode { get; set; }

        public abstract IEnumerable<MediaInfo> Apply(IEnumerable<MediaInfo> source);
    }
}
