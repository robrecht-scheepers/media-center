using MediaCenter.Helpers;
using MediaCenter.MVVM;
using MediaCenter.Repository;

namespace MediaCenter.Sessions
{
    public abstract class SessionViewModelBase : PropertyChangedNotifier
    {
        protected readonly IRepository Repository;
        protected readonly IWindowService WindowService;
        
        protected SessionViewModelBase(IRepository repository, IWindowService windowService, ShortcutService shortcutService)
        {
            Repository = repository;
            WindowService = windowService;
            ShortcutService = shortcutService;
        }

        public abstract string Name { get; }

        public ShortcutService ShortcutService { get; }
    }
}
