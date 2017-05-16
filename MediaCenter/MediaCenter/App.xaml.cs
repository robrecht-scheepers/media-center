﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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
            if (bootstrapper == null)
                throw new ArgumentNullException(nameof(bootstrapper));

            _bootstrapper = bootstrapper;
        }

        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => {
                _bootstrapper.Run();
            }));
        }
    }
}
