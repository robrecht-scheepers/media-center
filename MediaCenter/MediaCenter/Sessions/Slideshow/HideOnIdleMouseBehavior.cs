using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Threading;

namespace MediaCenter.Sessions.Slideshow
{
    public class HideOnIdleMouseBehavior : Behavior<SlideShowWindow>
    {
        public TimeSpan Interval { get { return (TimeSpan)GetValue(IntervalProperty); } set { SetValue(IntervalProperty, value); } }
        public static readonly DependencyProperty IntervalProperty = DependencyProperty.Register("Interval", typeof(TimeSpan), typeof(HideOnIdleMouseBehavior), new PropertyMetadata(null));

        private DispatcherTimer _activityTimer;
        private Cursor _hiddenCursor;
        
        protected override void OnAttached()
        {
            base.OnAttached();

            Hide();

            if (_activityTimer == null)
            {
                _activityTimer = new DispatcherTimer
                {
                    IsEnabled = true,
                    Interval = Interval
                };
                _activityTimer.Tick += (s,a) => Hide();

                AssociatedObject.MouseMove += AssociatedObjectOnMouseMove;
            }
        }

        protected override void OnDetaching()
        {
            _activityTimer.Stop();
            _activityTimer = null;
            AssociatedObject.MouseMove -= AssociatedObjectOnMouseMove;
            Show();

            base.OnDetaching();
        }

        private void AssociatedObjectOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            Show();

            _activityTimer.Stop();
            _activityTimer.Start();
        }

        private void Hide()
        {
            _hiddenCursor = AssociatedObject.Cursor;
            AssociatedObject.Cursor = Cursors.None;
            if(AssociatedObject.Controls != null)
                AssociatedObject.Controls.Visibility = Visibility.Hidden;

        }

        private void Show()
        {
            AssociatedObject.Cursor = _hiddenCursor;
            if (AssociatedObject.Controls != null)
                AssociatedObject.Controls.Visibility = Visibility.Visible;
        }
    }
}
