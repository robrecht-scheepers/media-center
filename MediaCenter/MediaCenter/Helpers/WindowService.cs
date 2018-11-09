using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MediaCenter.MVVM;
using MediaCenter.Sessions.Slideshow;

namespace MediaCenter.Helpers
{
    public class WindowService : IWindowService
    {
        private readonly Window _ownerWindow;

        public WindowService(Window ownerWindow)
        {
            _ownerWindow = ownerWindow;
        }

        public void OpenDialogWindow(PropertyChangedNotifier dataContext)
        {
            if (dataContext is SlideShowViewModel)
            {
                var window = new SlideShowWindow
                {
                    DataContext = dataContext,
                    Owner = _ownerWindow
                };
                window.ShowDialog();
            }
        }

        public void ShowMessage(string message, string caption)
        {
            MessageBox.Show(message, caption);
        }

        public bool AskConfirmation(string message)
        {
            var result = MessageBox.Show(message, "Bestätigung", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            return result == MessageBoxResult.Yes;
        }
    }
}
