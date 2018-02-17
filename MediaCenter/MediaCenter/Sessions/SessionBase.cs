using MediaCenter.MVVM;
using MediaCenter.Repository;

namespace MediaCenter.Sessions
{
    public abstract class SessionBase : PropertyChangedNotifier
    {
        public IRepository Repository;
        protected SessionBase(IRepository repository)
        {
            Repository = repository;
        }
    }
}
