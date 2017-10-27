using System.Collections.ObjectModel;
using MediaCenter.MVVM;
using MediaCenter.Repository;
using MediaCenter.Sessions;
using MediaCenter.Sessions.Query;
using MediaCenter.Sessions.Query.Filters;
using MediaCenter.Sessions.Staging;
using System.Reflection;

namespace MediaCenter
{
    public class MainWindowViewModel : PropertyChangedNotifier
    {
        
        public MainWindowViewModel(IRepository repository)
        {
            Sessions = new ObservableCollection<SessionViewModelBase>();
            Repository = repository;
            RepositoryViewModel = new RepositoryViewModel(Repository);

            // debug code
            var debugSession = new StagingSessionViewModel(new StagingSession(Repository));
            Sessions.Add(debugSession);
            
        }

        public string AppVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public IRepository Repository { get; }
        public RepositoryViewModel RepositoryViewModel { get; }
        
        public ObservableCollection<SessionViewModelBase> Sessions { get; private set; }

        // Start new staging session
        private RelayCommand _newStagingSessionCommand;
        public RelayCommand NewStagingSessionCommand => _newStagingSessionCommand ?? (_newStagingSessionCommand = new RelayCommand(CreateNewStagingSession));
        private void CreateNewStagingSession()
        {
            Sessions.Add(new StagingSessionViewModel(new StagingSession(Repository)));
        }

        // Start new query session
        private RelayCommand _newQuerySessionCommand;
        public RelayCommand NewQuerySessionCommand
            => _newQuerySessionCommand ?? (_newQuerySessionCommand = new RelayCommand(StartNewQuerySessionCommand));

        private void StartNewQuerySessionCommand()
        {
            Sessions.Add(new QuerySessionViewModel(new QuerySession(Repository)));
        }

        private RelayCommand<SessionViewModelBase> _closeSessionCommand;

        public RelayCommand<SessionViewModelBase> CloseSessionCommand =>
            _closeSessionCommand ?? (_closeSessionCommand = new RelayCommand<SessionViewModelBase>(CloseSession));

        public void CloseSession(SessionViewModelBase session)
        {
            Sessions.Remove(session);
        }
    }
}
