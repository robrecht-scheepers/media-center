using System;
using MediaCenter.MVVM;

namespace MediaCenter.Helpers
{
    public interface IWindowService
    {
        Guid OpenWindow(PropertyChangedNotifier dataContext, bool dialog, Action onWindowClosed = null);

        void CloseWindow(Guid windowId);

        void ShowMessage(string message, string caption);

        bool AskConfirmation(string message);
    }
}
