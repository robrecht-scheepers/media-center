using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Windows;
using MediaCenter.Helpers;
using MediaCenter.Properties;
using MediaCenter.Repository;

namespace MediaCenter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IRepository _repository;
        
        private async void ApplicationStartup(object sender, StartupEventArgs e)
        {
            var splashScreen = new SplashScreenWindow("Initializing repository");
            splashScreen.Show();

            MainWindowViewModel mainViewModel;

            var mainView = new MainWindow();
            var windowService = new WindowService(mainView);

            var repositoryPath = Settings.Default.RepositoryPath;
            var cachePath = Settings.Default.CachePath;

            var cache = new DbRepository(cachePath);
            if (DbRepository.CheckRepositoryConnection(repositoryPath))
            {
                _repository = new DbRepository(repositoryPath, cache);
                mainViewModel = new MainWindowViewModel(_repository, windowService, false);
            }
            else
            {
                mainViewModel = new MainWindowViewModel(cache, windowService, true);
            }
            
            await _repository.Initialize();
            mainView.DataContext = mainViewModel;
            mainView.Closing += MainViewOnClosing;

            splashScreen.Close();
            MainWindow = mainView;
            mainView.Show();
        }

        private void MainViewOnClosing(object sender, CancelEventArgs e)
        {
            var waitScreen = new SplashScreenWindow("waiting for backgrond operations") {Owner = MainWindow};
            waitScreen.Show();
            _repository?.Close();
            waitScreen.Close();
        }

        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            Settings.Default.Save();
        }
    }
}
