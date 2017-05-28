using System.Collections.Generic;
using MediaCenter.MVVM;
using MediaCenter.Repository;

namespace MediaCenter.Sessions.Query
{
    public abstract class Criterion : Observable
    {
        public abstract IEnumerable<CatalogItem> Filter(IEnumerable<CatalogItem> source, FilterMode filterMode);
    }
}
