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
    public class SlideShowViewModel : PropertyChangedNotifier
    {
        private Timer _timer;
        

        public QuerySessionViewModel QuerySessionViewModel { get; }

        public SlideShowViewModel(QuerySessionViewModel querySessionViewModel)
        {
            QuerySessionViewModel = querySessionViewModel;
            Interval = 1;
        }

        private int _interval;
        public int Interval
        {
            get { return _interval; }
            set { SetValue(ref _interval, value); }
        }

        public void Start()
        {
            _timer = new Timer(1000*Interval)
            {
                AutoReset = false
            };
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss tt ss.fff")} | Timer tick");
            QuerySessionViewModel.SelectNextImageCommand.Execute(null);
            _timer.Start();
        }
    }
}
