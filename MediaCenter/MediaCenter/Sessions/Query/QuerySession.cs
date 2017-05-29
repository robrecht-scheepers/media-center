using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.RightsManagement;
using MediaCenter.Repository;

namespace MediaCenter.Sessions.Query
{
    public class QuerySession : SessionBase
    {
        public QuerySession(RemoteRepository repository) : base(repository)
        {
            Filters = new ObservableCollection<Filter>();
            QueryResult = new ObservableCollection<SessionItem>();
        }

        public ObservableCollection<Filter> Filters { get; } 

        public ObservableCollection<SessionItem> QueryResult { get; private set; }

        public void ExecuteQuery()
        {
            var infos = Filters.Aggregate(Repository.Catalog, (current, filter) => filter.Apply(current));

            QueryResult.Clear();
            foreach (var mediaInfo in infos)
            {
                QueryResult.Add(new SessionItem { Info = mediaInfo });
            }
        }
    }
}
