using System.Collections.ObjectModel;
using MediaCenter.MVVM;
using MediaCenter.Repository;
using MediaCenter.Sessions;
using MediaCenter.Sessions.Query;
using MediaCenter.Sessions.Staging;
using System.Reflection;
using System;
using System.Linq;

namespace MediaCenter
{
    public class MainWindowViewModel : PropertyChangedNotifier
    {
        public MainWindowViewModel(IRepository repository)
        {
            Sessions = new ObservableCollection<SessionTabViewModel>();
            Repository = repository;
            RepositoryViewModel = new RepositoryViewModel(Repository);
            CreateNewSessionTab();
            SelectedSessionTab = Sessions.First();
        }


        public string AppVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public IRepository Repository { get; }
        public RepositoryViewModel RepositoryViewModel { get; }
        
        public ObservableCollection<SessionTabViewModel> Sessions { get; private set; }

        public SessionTabViewModel SelectedSessionTab
        {
            get { return _selectedSessionTab; }
            set { SetValue(ref _selectedSessionTab, value); }
        }

        private RelayCommand<SessionTabViewModel> _closeSessionCommand;
        private SessionTabViewModel _selectedSessionTab;

        public RelayCommand<SessionTabViewModel> CloseSessionCommand =>
            _closeSessionCommand ?? (_closeSessionCommand = new RelayCommand<SessionTabViewModel>(CloseSession));
        public void CloseSession(SessionTabViewModel session)
        {
            Sessions.Remove(session);
        }

        private void CreateNewSessionTab()
        {
            var newSessionTab = new SessionTabViewModel(Repository);
            newSessionTab.SessionCreated += NewSessionTabOnSessionCreated;
            Sessions.Add(newSessionTab);
        }

        private void NewSessionTabOnSessionCreated(object sender, EventArgs eventArgs)
        {
            ((SessionTabViewModel)sender).SessionCreated -= NewSessionTabOnSessionCreated;
            CreateNewSessionTab();
        }
    }
}
