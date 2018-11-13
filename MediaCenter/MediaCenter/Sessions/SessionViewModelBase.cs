using MediaCenter.Helpers;
using MediaCenter.MVVM;

namespace MediaCenter.Sessions
{
    public abstract class SessionViewModelBase : PropertyChangedNotifier
    {
        protected readonly SessionBase Session;
        protected readonly IWindowService WindowService;

        protected SessionViewModelBase(SessionBase session, IWindowService windowService)
        {
            Session = session;
            WindowService = windowService;
        }

        public abstract string Name { get; }
    }
}
