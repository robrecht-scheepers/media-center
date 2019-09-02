using System;
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
            var splashScreen = new SplashScreenWindow();
            splashScreen.Show();

            MainWindowViewModel mainViewModel;

            var mainView = new MainWindow();
            var windowService = new WindowService(mainView);

            var repositoryPath = Settings.Default.RepositoryPath;
            var cachePath = Settings.Default.CachePath;

            var cache = new DbRepository(cachePath);
            if (DbRepository.CheckRepositorConnection(repositoryPath))
            {
                _repository = new DbRepository(repositoryPath, cache);
                mainViewModel = new MainWindowViewModel(_repository, windowService, false);
            }
            else
            {
                _repository = cache;
                mainViewModel = new MainWindowViewModel(_repository, windowService, true);
            }

            await _repository.Initialize();
            mainView.DataContext = mainViewModel;

            splashScreen.Close();
            mainView.Show();
        }

        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            Settings.Default.Save();
        }
    }
}
