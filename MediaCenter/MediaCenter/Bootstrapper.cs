using System.Configuration;
using System.Threading.Tasks;
using MediaCenter.Repository;

namespace MediaCenter
{
    public class Bootstrapper
    {
        private IRepository _repository;

        public async Task Run()
        {
            // dummy code until repository management logic is ready
            _repository = new RemoteRepository(ConfigurationManager.AppSettings["RemoteStore"],
                ConfigurationManager.AppSettings["LocalStore"],ConfigurationManager.AppSettings["LocalCache"]);

            var repositoryTask = _repository.Initialize();

            // TDO: move back to the end of the method, when debug logic is not needed anymore
            await repositoryTask;

            var mainViewModel = new MainWindowViewModel(_repository);
            var mainView = new MainWindow {DataContext = mainViewModel};
            mainView.Show();
        }

        public void Exit()
        {
            
        }
    }
}
