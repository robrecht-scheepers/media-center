using MediaCenter.Repository;

namespace MediaCenter.Sessions.Query
{
    public class QuerySession : SessionBase
    {
        public QuerySession(MediaRepository repository) : base(repository)
        {
        }
    }
}
