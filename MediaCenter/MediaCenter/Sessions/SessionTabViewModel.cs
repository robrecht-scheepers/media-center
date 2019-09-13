using System;
using MediaCenter.Helpers;
using MediaCenter.MVVM;
using MediaCenter.Repository;
using MediaCenter.Sessions.Query;
using MediaCenter.Sessions.Staging;

namespace MediaCenter.Sessions
{
    public class SessionTabViewModel : PropertyChangedNotifier
    {
        private IRepository _repository;
        private readonly IWindowService _windowService;
        
        private SessionViewModelBase _sessionViewModel;
        private RelayCommand _createQuerySessionCommand;
        private RelayCommand _createStagingSessionCommand;
        
        public SessionTabViewModel(IRepository repository, IWindowService windowService, ShortcutService shortcutService, bool readOnly)
        {
            _repository = repository;
            _windowService = windowService;
            ShortcutService = shortcutService;
            ReadOnly = readOnly;
        }

        public bool ReadOnly { get; }

        public ShortcutService ShortcutService { get; }

        public SessionViewModelBase SessionViewModel
        {
            get => _sessionViewModel;
            set => SetValue(ref _sessionViewModel, value);
        }

        public string Name => SessionViewModel?.Name ?? "...";

        public RelayCommand CreateQuerySessionCommand => _createQuerySessionCommand ?? (_createQuerySessionCommand = new RelayCommand(CreateQuerySession));
        private void CreateQuerySession()
        {
            SessionViewModel = new QuerySessionViewModel(_windowService, _repository, ShortcutService, ReadOnly);
            SessionCreated?.Invoke(this, EventArgs.Empty);
            RaisePropertyChanged("Name");
        }

        public RelayCommand CreateStagingSessionCommand => _createStagingSessionCommand ?? (_createStagingSessionCommand = new RelayCommand(CreateStagingSession, CanExecuteCreateStagingSession));

        private bool CanExecuteCreateStagingSession()
        {
            return !ReadOnly;
        }

        private void CreateStagingSession()
        {
            SessionViewModel = new StagingSessionViewModel(_repository, _windowService, ShortcutService);
            SessionCreated?.Invoke(this, EventArgs.Empty);
            RaisePropertyChanged("Name");
        }

        public event EventHandler SessionCreated;
    }
}
