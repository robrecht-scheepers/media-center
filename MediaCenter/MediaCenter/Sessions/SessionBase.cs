using MediaCenter.Helpers;
using MediaCenter.MVVM;
using MediaCenter.Repository;

namespace MediaCenter.Sessions
{
    public abstract class SessionBase : PropertyChangedNotifier
    {
        public readonly IRepository Repository;
        protected readonly IWindowService WindowService;

        protected SessionBase(IRepository repository, IWindowService windowService)
        {
            Repository = repository;
            WindowService = windowService;
        }
    }
}
