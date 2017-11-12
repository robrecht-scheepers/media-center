using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using MediaCenter.MVVM;
using MediaCenter.Sessions.Query;
using System.Collections.ObjectModel;
using MediaCenter.Media;
using MediaCenter.Repository;

namespace MediaCenter.Sessions.Slideshow
{
    public enum SlideshowStatus
    {
        Active,
        Paused,
        Stopped
    }

    public class SlideShowViewModel : QueryResultDetailViewModel
    {
        private Timer _timer;
        
        public SlideShowViewModel(ObservableCollection<MediaItem> queryResultItems, IRepository repository, int startIndex = 0) : base(queryResultItems, repository)
        {
            Interval = 4;
            Status = SlideshowStatus.Stopped;

            if (startIndex > 0)
                SelectedItem = QueryResultItems[startIndex];
        }

        private SlideshowStatus _status;

        public SlideshowStatus Status
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
                case SlideshowStatus.Active:
                    return;
                case SlideshowStatus.Paused:
                    Status = SlideshowStatus.Active;
                    _timer.Start();
                    break;
                case SlideshowStatus.Stopped:
                    InitializeTimer();
                    Status = SlideshowStatus.Active;
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
            if (Status == SlideshowStatus.Active)
            {
                _timer.Stop();
                Status = SlideshowStatus.Paused;
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
                case SlideshowStatus.Active:
                case SlideshowStatus.Paused:
                    Status = SlideshowStatus.Stopped;
                    _timer.Stop();
                    _timer.Dispose();
                    break;
                case SlideshowStatus.Stopped:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (SelectedItem == QueryResultItems.Last())
            {
                Stop();
                Close();
                return;
            }

            SelectNextItem();
            _timer.Start();
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
