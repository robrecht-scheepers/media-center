using System;
using System.Linq;
using System.Timers;
using MediaCenter.MVVM;
using MediaCenter.Sessions.Query;
using System.Collections.ObjectModel;
using MediaCenter.Media;
using MediaCenter.Repository;

namespace MediaCenter.Sessions.Slideshow
{
    public class SlideShowViewModel : QueryResultDetailViewModel
    {
        private Timer _timer;
        
        public SlideShowViewModel(ObservableCollection<MediaItem> queryResultItems, IRepository repository, int startIndex = 0) : base(queryResultItems, repository)
        {
            Interval = 4;
            Status = PlayState.Stopped;

            if (startIndex > 0)
                SelectedItem = QueryResultItems[startIndex];
        }

        private PlayState _status;

        public PlayState Status
        {
            get { return _status; }
            set { SetValue(ref _status, value); }
        }

        private int _interval;
        public int Interval
        {
            get { return _interval; }
            set { SetValue(ref _interval, value, IntervalChanged); }
        }

        private void IntervalChanged()
        {
            if(_timer == null)
                return;

            _timer.Interval = 1000 * Interval;
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
            switch (Status)
            {
                case PlayState.Playing:
                    return;
                case PlayState.Paused:
                    Status = PlayState.Playing;
                    _timer.Start();
                    break;
                case PlayState.Stopped:
                    InitializeTimer();
                    Status = PlayState.Playing;
                    _timer.Start();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private RelayCommand _pauseCommand;
        public RelayCommand PauseCommand => _pauseCommand ?? (_pauseCommand = new RelayCommand(Pause));
        public void Pause()
        {
            if (Status == PlayState.Playing)
            {
                _timer.Stop();
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
            if (SelectedItem != null && SelectedItem.MediaType == MediaType.Video)
            {
                ((VideoItemViewModel)SelectedItemViewModel).VideoPlayFinished -= SelectedVideoPlayFinished;
            }
            if (SelectedItem == QueryResultItems.Last())
            {
                Stop();
                Close();
                return;
            }

            SelectNextItem();
            if (SelectedItem.MediaType == MediaType.Video)
            {
                ((VideoItemViewModel)SelectedItemViewModel).VideoPlayFinished += SelectedVideoPlayFinished;
            }
            else
            {
                _timer.Start();
            }
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Next();
        }

        private void SelectedVideoPlayFinished(object sender, EventArgs e)
        {
            Next();
        }
        

        private RelayCommand _closeCommand;
        public RelayCommand CloseCOmmand => _closeCommand ?? (_closeCommand = new RelayCommand(Close));
        private void Close()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler CloseRequested;
        
    }
}
