using MediaCenter.MVVM;
using MediaCenter.Repository;

namespace MediaCenter.Sessions
{
    public abstract class SessionBase : PropertyChangedNotifier
    {
        public RemoteRepository Repository;
        protected SessionBase(RemoteRepository repository)
        {
            Repository = repository;
        }
    }
}
