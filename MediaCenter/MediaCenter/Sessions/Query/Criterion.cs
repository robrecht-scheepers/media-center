using System.Collections.Generic;
using MediaCenter.MediaItems;
using MediaCenter.MVVM;

namespace MediaCenter.Sessions.Query
{
    public abstract class Criterion : Observable
    {
        public abstract IEnumerable<MediaItem> Filter(IEnumerable<MediaItem> source, FilterMode filterMode);
    }
}
