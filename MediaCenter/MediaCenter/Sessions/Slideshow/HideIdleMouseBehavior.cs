using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Threading;

namespace MediaCenter.Sessions.Slideshow
{
    public class HideIdleMouseBehavior : Behavior<Window>
    {
        public TimeSpan Interval { get { return (TimeSpan)GetValue(IntervalProperty); } set { SetValue(IntervalProperty, value); } }
        public static readonly DependencyProperty IntervalProperty = DependencyProperty.Register("Interval", typeof(TimeSpan), typeof(HideIdleMouseBehavior), new PropertyMetadata(null));

        private DispatcherTimer _activityTimer;
        
        protected override void OnAttached()
        {
            base.OnAttached();

            HideCursor();

            if (_activityTimer == null)
            {
                _activityTimer = new DispatcherTimer
                {
                    IsEnabled = true,
                    Interval = Interval
                };
                _activityTimer.Tick += (s,a) => HideCursor();

                AssociatedObject.MouseMove += AssociatedObjectOnMouseMove;
            }
        }

        protected override void OnDetaching()
        {
            _activityTimer.Stop();
            _activityTimer = null;
            AssociatedObject.MouseMove -= AssociatedObjectOnMouseMove;
            ShowCursor();

            base.OnDetaching();
        }

        private void AssociatedObjectOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            ShowCursor();

            _activityTimer.Stop();
            _activityTimer.Start();
        }

        private void HideCursor()
        {
            AssociatedObject.Cursor = Cursors.None;
        }

        private void ShowCursor()
        {
            AssociatedObject.Cursor = Cursors.Arrow;
        }
    }
}
