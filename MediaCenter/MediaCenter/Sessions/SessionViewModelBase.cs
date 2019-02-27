using MediaCenter.Helpers;
using MediaCenter.MVVM;
using MediaCenter.Repository;

namespace MediaCenter.Sessions
{
    public abstract class SessionViewModelBase : PropertyChangedNotifier
    {
        protected readonly IRepository Repository;
        protected readonly IWindowService WindowService;

        protected SessionViewModelBase(IRepository repository, IWindowService windowService)
        {
            Repository = repository;
            WindowService = windowService;
        }

        public abstract string Name { get; }
    }
}
