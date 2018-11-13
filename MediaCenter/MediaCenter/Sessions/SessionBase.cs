using MediaCenter.Helpers;
using MediaCenter.MVVM;
using MediaCenter.Repository;

namespace MediaCenter.Sessions
{
    public abstract class SessionBase : PropertyChangedNotifier
    {
        public readonly IRepository Repository;
        
        protected SessionBase(IRepository repository)
        {
            Repository = repository;
        }
    }
}
