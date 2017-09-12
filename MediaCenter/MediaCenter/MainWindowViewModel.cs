using System.Collections.ObjectModel;
using MediaCenter.MVVM;
using MediaCenter.Repository;
using MediaCenter.Sessions;
using MediaCenter.Sessions.Query;
using MediaCenter.Sessions.Query.Filters;
using MediaCenter.Sessions.Staging;

namespace MediaCenter
{
    public class MainWindowViewModel : PropertyChangedNotifier
    {
        
        public MainWindowViewModel(IRepository repository)
        {
            Sessions = new ObservableCollection<SessionViewModelBase>();
            Repository = repository;

            // debug code
            var debugSession = new StagingSessionViewModel(new StagingSession(Repository));
            Sessions.Add(debugSession);
            
        }

        public IRepository Repository { get; }
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

    }
}
