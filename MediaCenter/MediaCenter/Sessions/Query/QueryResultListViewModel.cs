using MediaCenter.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCenter.Sessions.Query
{
    public class QueryResultListViewModel : QueryResultViewModel
    {
        public QueryResultListViewModel(ObservableCollection<MediaItem> queryResultItems) : base(queryResultItems)
        {
            
        }
    }
}
