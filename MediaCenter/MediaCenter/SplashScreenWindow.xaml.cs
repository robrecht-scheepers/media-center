using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MediaCenter
{
    /// <summary>
    /// Interaction logic for SplashScreenWindow.xaml
    /// </summary>
    public partial class SplashScreenWindow : Window
    {
        private DispatcherTimer _timer;
        private int _ellipsisLength;
        public SplashScreenWindow()
        {
            InitializeComponent();
            _timer = new DispatcherTimer(DispatcherPriority.Render);
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            _timer.Tick += TimerOnTick;
            _ellipsisLength = 0;
            _timer.Start();
        }

        private void TimerOnTick(object sender, EventArgs e)
        {
            _ellipsisLength = (_ellipsisLength + 1) % 4;
            var text = "";
            for (int i = 0; i < _ellipsisLength; i++)
            {
                text += ".";
            }
            AnimatedEllipsis.Text = text;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            
        }
    }
}
