using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MediaCenter.MVVM;
using MediaCenter.Sessions.Query;
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

        public void OpenWindow(PropertyChangedNotifier dataContext, bool dialog)
        {
            Window window = null;
            if (dataContext is QuerySessionViewModel)
            {
                window = new SlideShowWindow();
            }

            if(window == null)
                return;

            window.DataContext = dataContext;
            window.Owner = _ownerWindow;
            if(dialog)
                window.ShowDialog();
            else
                window.Show();
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
