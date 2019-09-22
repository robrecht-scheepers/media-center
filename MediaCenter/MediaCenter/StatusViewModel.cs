using System.ComponentModel;
using System.Timers;
using MediaCenter.Helpers;
using MediaCenter.MVVM;
using MediaCenter.Repository;

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

        public StatusViewModel(IRepository repository)
        {
            _repository = repository;
            Message = "";
            ShowProgress = false;
            Progress = 0;
            _timer = new Timer(MessageTimeout) {AutoReset = false};
            _timer.Elapsed += (s, a) => Message = "";
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
            Message = message;
        }

        public void StartProgress()
        {
            Progress = 0;
            ShowProgress = true;
        }

        /// <summary>
        /// Updates the currently running progress indication
        /// </summary>
        /// <param name="progress">Value between 0 and 100</param>
        public void UpdateProgress(int progress)
        {
            Progress = progress;
        }

        public void EndProgress()
        {
            ShowProgress = false;
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
    }
}
