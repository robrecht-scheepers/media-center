using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.Repository;

namespace MediaCenter
{
    public class Bootstrapper
    {
        private MediaRepository _repository;

        public async Task Run()
        {
            // dummy code until repositorx management logic is ready
            _repository = new Repository.MediaRepository(ConfigurationManager.AppSettings["RemoteStore"],
                ConfigurationManager.AppSettings["LocalStore"]);

            //Task synchronizeRepositoryTask = _repository.SyncFromRemote();

            var mainViewModel = new MainWindowViewModel(_repository);
            var mainView = new MainWindow {DataContext = mainViewModel};
            mainView.Show();
            //await synchronizeRepositoryTask;
        }

        public void Exit()
        {
            
        }
    }
}
