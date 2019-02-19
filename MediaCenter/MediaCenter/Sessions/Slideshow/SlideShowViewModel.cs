using System;
using System.Linq;
using System.Timers;
using MediaCenter.MVVM;
using MediaCenter.Sessions.Query;
using System.Collections.ObjectModel;
using System.Configuration;
using MediaCenter.Media;
using MediaCenter.Repository;

namespace MediaCenter.Sessions.Slideshow
{
    public class SlideShowViewModel : QueryResultDetailViewModel
    {
        private Timer _timer;
        
        public SlideShowViewModel(ObservableCollection<MediaItem> items, IRepository repository, MediaItem selectedItem) : base(items, repository, selectedItem)
        {
            Interval = Properties.Settings.Default.SlideshowInterval;
            Status = PlayState.Stopped;
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
            if(Status == PlayState.Playing)
                return;

            if(Status == PlayState.Stopped)
                InitializeTimer();

            if (SelectedItemViewModel.MediaItem.MediaType == MediaType.Image)
                _timer.Start();
            else // video
            {
                //((VideoItemViewModel)SelectedItemViewModel).VideoPlayFinished += SelectedVideoPlayFinished;
                //((VideoItemViewModel)SelectedItemViewModel).VideoPlayState = PlayState.Playing;
            }

            Status = PlayState.Playing;
        }

        private RelayCommand _pauseCommand;
        public RelayCommand PauseCommand => _pauseCommand ?? (_pauseCommand = new RelayCommand(Pause));
        public void Pause()
        {
            if (Status == PlayState.Playing)
            {
                if (SelectedItemViewModel.MediaItem.MediaType == MediaType.Image)
                    _timer.Stop();
                //else // video
                //    ((VideoItemViewModel) SelectedItemViewModel).VideoPlayState = PlayState.Paused;

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
                    //if (SelectedItemViewModel.MediaItem.MediaType == MediaType.Video)
                    //    ((VideoItemViewModel)SelectedItemViewModel).VideoPlayState = PlayState.Stopped;
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
                //((VideoItemViewModel)SelectedItemViewModel).VideoPlayFinished -= SelectedVideoPlayFinished;
            }
            if (SelectedItem == Items.Last())
            {
                Stop();
                Close();
                return;
            }

            SelectNextItem();
            if (SelectedItem.MediaType == MediaType.Video)
            {
                //((VideoItemViewModel)SelectedItemViewModel).VideoPlayFinished += SelectedVideoPlayFinished;
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
