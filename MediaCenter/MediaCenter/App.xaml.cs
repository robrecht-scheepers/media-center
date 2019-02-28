using System;
using System.Configuration;
using System.IO;
using System.Net.Mime;
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
            var repoPath = ConfigurationManager.AppSettings["RepositoryPath"];
            var dbPath = Path.Combine(repoPath, "db", "mc.db3");
            var mediaPath = Path.Combine(repoPath, "media");
            var thumbnailPath = Path.Combine(repoPath, "thumbnails");
            _repository = new DbRepository(dbPath, mediaPath, thumbnailPath, @"c:\TEMP\MC\TEST\Repo\");
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
