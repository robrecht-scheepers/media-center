using System.Collections.ObjectModel;
using System.Security.RightsManagement;
using MediaCenter.Repository;

namespace MediaCenter.Sessions.Query
{
    public class QuerySession : SessionBase
    {
        public QuerySession(RemoteRepository repository) : base(repository)
        {
            Criteria = new ObservableCollection<Filter>();
            QueryResult = new ObservableCollection<SessionMediaItem>();
        }

        public ObservableCollection<Filter> Criteria { get; } 

        public ObservableCollection<SessionMediaItem> QueryResult { get; }

        public void ExecuteQuery()
        {
            QueryResult.Clear();
            
            
        }
    }
}
