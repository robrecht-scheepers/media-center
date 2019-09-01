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
            var repoPath = Settings.Default.RepositoryPath;
            
            _repository = new DbRepository(repoPath);
            var repositoryTask = _repository.Initialize();

            var mainView = new MainWindow();
            var windowService = new WindowService(mainView); 
            var mainViewModel = new MainWindowViewModel(_repository,windowService);
            mainView.DataContext = mainViewModel;
            mainView.Show();

            await repositoryTask;
        }

        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            Settings.Default.Save();
        }
    }
}
