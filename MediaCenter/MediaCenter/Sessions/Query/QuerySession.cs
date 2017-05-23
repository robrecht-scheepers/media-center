using System.Collections.ObjectModel;
using System.Security.RightsManagement;
using MediaCenter.MediaItems;
using MediaCenter.Repository;

namespace MediaCenter.Sessions.Query
{
    public class QuerySession : SessionBase
    {
        public QuerySession(MediaRepository repository) : base(repository)
        {
            Criteria = new ObservableCollection<Criterion>();
            QueryResult = new ObservableCollection<MediaItem>();
        }

        public ObservableCollection<Criterion> Criteria { get; } 

        public ObservableCollection<MediaItem> QueryResult { get; }

        public void ExecuteQuery()
        {
            QueryResult.Clear();
            
            
        }
    }
}
