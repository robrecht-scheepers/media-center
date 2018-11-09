using MediaCenter.MVVM;

namespace MediaCenter.Helpers
{
    public interface IWindowService
    {
        void OpenDialogWindow(PropertyChangedNotifier dataContext);

        void ShowMessage(string message, string caption);

        bool AskConfirmation(string message);
    }
}
