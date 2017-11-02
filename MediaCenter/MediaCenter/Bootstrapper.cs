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
               
            var mainViewModel = new MainWindowViewModel(_repository);
            var mainView = new MainWindow {DataContext = mainViewModel};
            mainView.Show();

            await repositoryTask;
        }

        public void Exit()
        {
            
        }
    }
}
