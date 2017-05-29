using System.Collections.Generic;
using MediaCenter.MVVM;
using MediaCenter.Repository;

namespace MediaCenter.Sessions.Query
{
    public abstract class Filter : Observable
    {
        public abstract IEnumerable<CatalogItem> Apply(IEnumerable<CatalogItem> source, FilterMode filterMode);
    }
}
