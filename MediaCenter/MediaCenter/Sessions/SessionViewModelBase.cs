using MediaCenter.Helpers;
using MediaCenter.MVVM;
using MediaCenter.Repository;

namespace MediaCenter.Sessions
{
    public abstract class SessionViewModelBase : PropertyChangedNotifier
    {
        protected readonly IRepository Repository;
        protected readonly IWindowService WindowService;
        protected readonly IStatusService StatusService;
        
        protected SessionViewModelBase(IRepository repository, IWindowService windowService, ShortcutService shortcutService, IStatusService statusService)
        {
            Repository = repository;
            WindowService = windowService;
            ShortcutService = shortcutService;
            StatusService = statusService;
        }

        public abstract string Name { get; }

        public ShortcutService ShortcutService { get; }
    }
}
