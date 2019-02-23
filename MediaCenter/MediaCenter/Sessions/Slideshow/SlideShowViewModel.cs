using System;
using System.Timers;
using MediaCenter.MVVM;
using MediaCenter.Sessions.Query;
using MediaCenter.Helpers;
using MediaCenter.Media;

namespace MediaCenter.Sessions.Slideshow
{
    public class SlideShowViewModel : PropertyChangedNotifier
    {
        private readonly QuerySessionViewModel _querySessionViewModel;
        private readonly IWindowService _windowService;
        private Timer _timer;
        private PlayState _status;
        private int _interval;

        public SlideShowViewModel(QuerySessionViewModel querySessionViewModel, IWindowService windowService)
        {
            _querySessionViewModel = querySessionViewModel;
            _windowService = windowService;
            Interval = Properties.Settings.Default.SlideshowInterval;
            Status = PlayState.Stopped;
            MediaItemViewModel.VideoPlayFinished += OnVideoFinished;
        }

        public QueryResultViewModel QueryResultViewModel => _querySessionViewModel.QueryResultViewModel;
        public MediaItemViewModel MediaItemViewModel => _querySessionViewModel.DetailItem;
        public EditMediaInfoViewModel EditMediaInfoViewModel => _querySessionViewModel.EditMediaInfoViewModel;

        public RelayCommand SelectNextItemCommand => QueryResultViewModel.SelectNextItemCommand;
        public RelayCommand SelectPreviousItemCommand => QueryResultViewModel.SelectPreviousItemCommand;
        
        public PlayState Status
        {
            get => _status;
            set => SetValue(ref _status, value);
        }
        
        public int Interval
        {
            get => _interval;
            set => SetValue(ref _interval, value, IntervalChanged);
        }

        public Guid WindowId { get; set; }

        private void IntervalChanged()
        {
            Properties.Settings.Default.SlideshowInterval = Interval;
            if(_timer == null)
                return;

            _timer.Interval = 1000 * Interval;

            // check if the timer is running before restarting as the timer is not used for videos
            if (_timer.Enabled) 
            {
                _timer.Stop();
                _timer.Start();
            }
        }

        private void InitializeTimer()
        {
            _timer = new Timer(1000*Interval)
            {
                AutoReset = false
            };
            _timer.Elapsed += TimerOnElapsed;
        }

        public void Start()
        {
            if (Status == PlayState.Playing)
                return;

            if (Status == PlayState.Stopped)
                InitializeTimer();

            if (MediaItemViewModel.MediaItem.MediaType == MediaType.Image)
                _timer.Start();

            Status = PlayState.Playing;
        }

        private RelayCommand _pauseCommand;
        public RelayCommand PauseCommand => _pauseCommand ?? (_pauseCommand = new RelayCommand(Pause));
        public void Pause()
        {
            if (Status == PlayState.Playing)
            {
                if (MediaItemViewModel.MediaItem.MediaType == MediaType.Image)
                    _timer.Stop();
                else // video
                    MediaItemViewModel.VideoPlayState = PlayState.Paused;

                Status = PlayState.Paused;
            }
        }

        private RelayCommand _playCommand;
        public RelayCommand PlayCommand => _playCommand ?? (_playCommand = new RelayCommand(Play));
        public void Play()
        {
            Start();
        }

        public void Stop()
        {
            switch (Status)
            {
                case PlayState.Playing:
                case PlayState.Paused:
                    Status = PlayState.Stopped;
                    MediaItemViewModel.VideoPlayFinished -= OnVideoFinished;
                    if (MediaItemViewModel.MediaItem.MediaType == MediaType.Video)
                        MediaItemViewModel.VideoPlayState = PlayState.Stopped;
                    _timer.Stop();
                    _timer.Dispose();
                    break;
                case PlayState.Stopped:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Next()
        {
            if (!QueryResultViewModel.CanExecuteSelectNextItem())
            {
                Stop();
                Close();
                return;
            }

            QueryResultViewModel.SelectNextItem();
            if (MediaItemViewModel.MediaItem.MediaType == MediaType.Image)
            {
                _timer.Start();
            }
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Next();
        }

        private void OnVideoFinished(object sender, EventArgs e)
        {
            Next();
        }
        

        private RelayCommand _closeCommand;
        public RelayCommand CloseCommand => _closeCommand ?? (_closeCommand = new RelayCommand(Close));
        private void Close()
        {
            _windowService.CloseWindow(WindowId);
        }
        
    }
}
