using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace VideoPlayerPOC
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

        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            _bootstrapper.Run();
        }

        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            _bootstrapper.Exit();
        }
    }
}
