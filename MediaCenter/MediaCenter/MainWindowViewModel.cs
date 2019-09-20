using System.Collections.ObjectModel;
using MediaCenter.MVVM;
using MediaCenter.Repository;
using MediaCenter.Sessions;
using MediaCenter.Sessions.Query;
using MediaCenter.Sessions.Staging;
using System.Reflection;
using System;
using System.Linq;
using MediaCenter.Helpers;

namespace MediaCenter
{
    public class MainWindowViewModel : PropertyChangedNotifier
    {
        private readonly IWindowService _windowService;
        private readonly IRepository _repository;
        private RelayCommand<SessionTabViewModel> _closeSessionCommand;
        private SessionTabViewModel _selectedSessionTab;


        public MainWindowViewModel(IRepository repository, IWindowService windowService, bool readOnly)
        {
            ReadOnly = readOnly;
            _windowService = windowService;
            _repository = repository;
            

            Sessions = new ObservableCollection<SessionTabViewModel>();
            StatusViewModel = new StatusViewModel(_repository);
            CreateEmptySessionTab();
            SelectedSessionTab = Sessions.First();
        }


        public string AppVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public StatusViewModel StatusViewModel { get; }
        public ObservableCollection<SessionTabViewModel> Sessions { get; }
        public bool ReadOnly { get; }

        public SessionTabViewModel SelectedSessionTab
        {
            get => _selectedSessionTab;
            set => SetValue(ref _selectedSessionTab, value);
        }

        
        public RelayCommand<SessionTabViewModel> CloseSessionCommand =>
            _closeSessionCommand ?? (_closeSessionCommand = new RelayCommand<SessionTabViewModel>(CloseSession));
        public void CloseSession(SessionTabViewModel session)
        {
            Sessions.Remove(session);
        }

        private void CreateEmptySessionTab()
        {
            var newSessionTab = new SessionTabViewModel(_repository,_windowService, new ShortcutService(), StatusViewModel, ReadOnly);
            newSessionTab.SessionCreated += NewSessionTabOnSessionCreated;
            Sessions.Add(newSessionTab);
        }

        private void NewSessionTabOnSessionCreated(object sender, EventArgs eventArgs)
        {
            ((SessionTabViewModel)sender).SessionCreated -= NewSessionTabOnSessionCreated;
            CreateEmptySessionTab();
        }
    }
}
