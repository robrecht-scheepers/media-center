using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;
using MediaCenter.Helpers;
using MediaCenter.Media;
using MediaCenter.MVVM;
using MediaCenter.Repository;
using MediaCenter.Sessions.Filters;

namespace MediaCenter
{
    public class StatusViewModel : PropertyChangedNotifier, IStatusService
    {
        private const int MessageTimeout = 5000;

        private readonly IRepository _repository;
        private bool _showProgress;
        private int _progress;
        private string _message;
        private Timer _timer;
        private string _repositoryLocation;
        private int _repositoryTotalCount;
        private int _repositoryImageCount;
        private int _repositoryVideoCount;

        public StatusViewModel(IRepository repository)
        {
            _repository = repository;
            Message = "";
            ShowProgress = false;
            Progress = 0;
            _timer = new Timer(MessageTimeout) {AutoReset = false};
            _timer.Elapsed += (s, a) => Message = "";

            _repository.CollectionChanged += (s, a) => UpdateRepositoryInfo();
            UpdateRepositoryInfo();
        }
        
        public void PostStatusMessage(string message, bool keep = false)
        {
            if (keep)
            {
                _timer.Stop();
            }
            else
            {
                _timer.Start();
            }
            Dispatcher.CurrentDispatcher.Invoke(() => Message = message);
        }

        public void ClearStatusMessage()
        {
            Dispatcher.CurrentDispatcher.Invoke(() => Message = "");
        }

        public void StartProgress()
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                Progress = 0;
                ShowProgress = true;
            });
        }

        /// <summary>
        /// Updates the currently running progress indication
        /// </summary>
        /// <param name="progress">Value between 0 and 100</param>
        public void UpdateProgress(int progress)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>  Progress = progress);
        }

        public void EndProgress()
        {
            Dispatcher.CurrentDispatcher.Invoke(() => ShowProgress = false);
        }

        public bool ShowProgress
        {
            get => _showProgress;
            set => SetValue(ref _showProgress, value);
        }

        public int Progress
        {
            get => _progress;
            set => SetValue(ref _progress, value);
        }

        public string Message
        {
            get => _message;
            set => SetValue(ref _message, value);
        }

        public string RepositoryLocation
        {
            get => _repositoryLocation;
            set => SetValue(ref _repositoryLocation, value);
        }

        public int RepositoryTotalCount
        {
            get => _repositoryTotalCount;
            set => SetValue(ref _repositoryTotalCount, value);
        }

        public int RepositoryImageCount
        {
            get => _repositoryImageCount;
            set => SetValue(ref _repositoryImageCount, value);
        }

        public int RepositoryVideoCount
        {
            get => _repositoryVideoCount;
            set => SetValue(ref _repositoryVideoCount, value);
        }

        private void UpdateRepositoryInfo()
        {
            RepositoryLocation = _repository.Location.LocalPath;
            RepositoryImageCount = _repository.ImageCount;
            RepositoryVideoCount = _repository.VideoCount;
            RepositoryTotalCount = RepositoryImageCount + RepositoryVideoCount;
        }

        public string AppVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }
}
