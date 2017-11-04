using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using MediaCenter.MVVM;
using MediaCenter.Sessions.Query;

namespace MediaCenter.Sessions.Slideshow
{
    public enum SlideshowStatus
    {
        Active,
        Paused,
        Stopped
    }

    public class SlideShowViewModel : PropertyChangedNotifier
    {
        private Timer _timer;

        public QuerySessionViewModel QuerySessionViewModel { get; }

        public SlideShowViewModel(QuerySessionViewModel querySessionViewModel)
        {
            QuerySessionViewModel = querySessionViewModel;
            Interval = 4;
            Status = SlideshowStatus.Stopped;
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
            set { SetValue(ref _interval, value); }
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

        private RelayCommand _pauseResumeCommand;
        public RelayCommand PauseResumeCommand => _pauseResumeCommand ?? (_pauseResumeCommand = new RelayCommand(PauseResume));
        public void PauseResume()
        {
            switch (Status)
            {
                case SlideshowStatus.Active:
                    _timer.Stop();
                    Status = SlideshowStatus.Paused;
                    break;
                case SlideshowStatus.Paused:
                    Start();
                    break;
                case SlideshowStatus.Stopped:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
            //Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss tt ss.fff")} | Timer tick");
            //if (!QuerySessionViewModel.SelectNextImageCommand.CanExecute(null))
            //{
            //    Close();
            //}
            //QuerySessionViewModel.SelectNextImageCommand.Execute(null);
            //if(Status == SlideshowStatus.Active)
            //    _timer.Start();
        }

        private RelayCommand _closeCommand;
        public RelayCommand CloseCOmmand => _closeCommand ?? (_closeCommand = new RelayCommand(Close));
        private void Close()
        {
            QuerySessionViewModel.CloseSlideShowCommand.Execute(null);
        }

        private RelayCommand _nextImageCommand;
        public RelayCommand NextImageCommand => _nextImageCommand ?? (_nextImageCommand = new RelayCommand(NextImage, CanExecuteNextImage));
        private void NextImage()
        {

            if(Status == SlideshowStatus.Active)
                _timer.Stop();

            //QuerySessionViewModel.SelectNextImageCommand.Execute(null);

            if (Status == SlideshowStatus.Active)
                _timer.Start();
        }
        private bool CanExecuteNextImage()
        {
            return true;
            //return QuerySessionViewModel.SelectNextImageCommand.CanExecute(null);
        }

        private RelayCommand _previousImageCommand;
        public RelayCommand PreviousImageCommand => _previousImageCommand ?? (_previousImageCommand = new RelayCommand(PreviousImage, CanExecutePreviousImage));
        private void PreviousImage()
        {

            if (Status == SlideshowStatus.Active)
                _timer.Stop();

            //QuerySessionViewModel.SelectPreviousImageCommand.Execute(null);

            if (Status == SlideshowStatus.Active)
                _timer.Start();
        }
        private bool CanExecutePreviousImage()
        {
            return true;
            //return QuerySessionViewModel.SelectPreviousImageCommand.CanExecute(null);
        }
    }
}
