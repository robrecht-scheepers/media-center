using MediaCenter.MVVM;

namespace MediaCenter.Helpers
{
    public interface IWindowService
    {
        void OpenWindow(PropertyChangedNotifier dataContext, bool dialog);

        void ShowMessage(string message, string caption);

        bool AskConfirmation(string message);
    }
}
