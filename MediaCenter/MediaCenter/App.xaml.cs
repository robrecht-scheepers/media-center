using System;
using System.Windows;
using MediaCenter.Properties;

namespace MediaCenter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Bootstrapper _bootstrapper;

        public App() : this(new Bootstrapper()) { }
        public App(Bootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));
        }

        private async void ApplicationStartup(object sender, StartupEventArgs e)
        {
            await _bootstrapper.Run();
        }

        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            _bootstrapper.Exit();
            Settings.Default.Save();
        }
    }
}
