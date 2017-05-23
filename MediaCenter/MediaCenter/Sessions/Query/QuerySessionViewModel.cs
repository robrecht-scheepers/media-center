namespace MediaCenter.Sessions.Query
{
    public class QuerySessionViewModel : SessionViewModelBase
    {
        public QuerySessionViewModel(SessionBase session) : base(session)
        {
        }

        protected override string CreateNameForSession(SessionBase session)
        {
            return "Query 1";
        }
    }
}
