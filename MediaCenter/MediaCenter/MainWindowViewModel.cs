﻿using System.Collections.ObjectModel;
using MediaCenter.MVVM;
using MediaCenter.Repository;
using MediaCenter.Sessions;
using MediaCenter.Sessions.Query;
using MediaCenter.Sessions.Staging;
using System.Reflection;
using System;
using System.Linq;
using System.Threading.Tasks;
using MediaCenter.Helpers;

namespace MediaCenter
{
    public class MainWindowViewModel : PropertyChangedNotifier
    {
        private readonly IWindowService _windowService;
        private readonly IRepository _repository;
        private AsyncRelayCommand<SessionTabViewModel> _closeSessionCommand;
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


        
        public StatusViewModel StatusViewModel { get; }
        public ObservableCollection<SessionTabViewModel> Sessions { get; }
        public bool ReadOnly { get; }

        public SessionTabViewModel SelectedSessionTab
        {
            get => _selectedSessionTab;
            set => SetValue(ref _selectedSessionTab, value);
        }

        
        public AsyncRelayCommand<SessionTabViewModel> CloseSessionCommand =>
            _closeSessionCommand ?? (_closeSessionCommand = new AsyncRelayCommand<SessionTabViewModel>(CloseSession));
        public async Task CloseSession(SessionTabViewModel session)
        {
            await session.Close();
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
