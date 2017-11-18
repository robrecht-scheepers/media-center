using System;
using MediaCenter.MVVM;
using MediaCenter.Repository;
using MediaCenter.Sessions.Query;
using MediaCenter.Sessions.Staging;

namespace MediaCenter.Sessions
{
    public class SessionTabViewModel : PropertyChangedNotifier
    {
        private IRepository _repository;
        private SessionViewModelBase _sessionViewModel;
        private RelayCommand _createQuerySessionCommand;
        private RelayCommand _createStagingSessionCommand;

        public SessionTabViewModel(IRepository repository)
        {
            _repository = repository;
        }

        public SessionViewModelBase SessionViewModel
        {
            get { return _sessionViewModel; }
            set { SetValue(ref _sessionViewModel, value); }
        }

        public string Name => SessionViewModel?.Name ?? "Start new session";

        public RelayCommand CreateQuerySessionCommand => _createQuerySessionCommand ?? (_createQuerySessionCommand = new RelayCommand(CreateQuerySession));
        private void CreateQuerySession()
        {
            SessionViewModel = new QuerySessionViewModel(new QuerySession(_repository));
            SessionCreated?.Invoke(this, EventArgs.Empty);
            RaisePropertyChanged("Name");
        }

        public RelayCommand CreateStagingSessionCommand => _createStagingSessionCommand ?? (_createStagingSessionCommand = new RelayCommand(CreateStagingSession));

        private void CreateStagingSession()
        {
            SessionViewModel = new StagingSessionViewModel(new StagingSession(_repository));
            SessionCreated?.Invoke(this, EventArgs.Empty);
            RaisePropertyChanged("Name");
        }

        public event EventHandler SessionCreated;
    }
}
