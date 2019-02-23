using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MediaCenter.MVVM;
using MediaCenter.Sessions.Query;
using MediaCenter.Sessions.Slideshow;
using MessageBox = System.Windows.MessageBox;

namespace MediaCenter.Helpers
{
    public class WindowService : IWindowService
    {
        private readonly Window _ownerWindow;
        private readonly List<(Guid, Window, Action)> _windows;

        public WindowService(Window ownerWindow)
        {
            _ownerWindow = ownerWindow;
            _windows = new List<(Guid, Window, Action)>();
        }

        public Guid OpenWindow(PropertyChangedNotifier dataContext, bool dialog, Action onWindowClosed = null)
        {
            Window window = null;
            if (dataContext is SlideShowViewModel)
            {
                window = new SlideShowWindow();
            }

            if(window == null)
                return Guid.Empty;

            window.DataContext = dataContext;
            window.Owner = _ownerWindow;
            window.Closed += WindowOnClosed;
            var guid = Guid.NewGuid();
            _windows.Add((guid, window, onWindowClosed));

            if (dialog)
                window.ShowDialog();
            else
                window.Show();

            return guid;
        }

        public void CloseWindow(Guid windowId)
        {
            _windows.FirstOrDefault(x => x.Item1 == windowId).Item2?.Close();
        }

        private void WindowOnClosed(object sender, EventArgs e)
        {
            var window = (Window) sender;
            window.Closed -= WindowOnClosed;
            _windows.FirstOrDefault(x => x.Item2 == window).Item3?.Invoke();
            _windows.RemoveAll(x => x.Item2 == window);
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
