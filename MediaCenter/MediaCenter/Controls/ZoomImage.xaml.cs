using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MediaCenter.Controls
{
    /// <summary>
    /// Interaction logic for ZoomImage.xaml
    /// </summary>
    public partial class ZoomImage : UserControl
    {
        private Point? _lastCenterPositionOnTarget;
        private Point? _lastMousePositionOnTarget;
        private Point? _lastDragPoint;

        public ZoomImage()
        {
            InitializeComponent();

            scaleTransform.CenterX = 0.5;
            scaleTransform.CenterY = 0.5;

            scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            scrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;
            scrollViewer.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            scrollViewer.MouseMove += OnMouseMove;
            scrollViewer.MouseLeftButtonUp += OnMouseLeftButtonUp;

            slider.ValueChanged += OnSliderValueChanged;
            slider.Value = 1;
        }

        void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(scrollViewer);
            if (mousePos.X <= scrollViewer.ViewportWidth
                && mousePos.Y < scrollViewer.ViewportHeight) //make sure we still can use the scrollbars
            {
                scrollViewer.Cursor = Cursors.SizeAll;
                _lastDragPoint = mousePos;
                Mouse.Capture(scrollViewer);
            }
        }
        void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_lastDragPoint.HasValue)
            {
                Point posNow = e.GetPosition(scrollViewer);

                double dX = posNow.X - _lastDragPoint.Value.X;
                double dY = posNow.Y - _lastDragPoint.Value.Y;

                _lastDragPoint = posNow;

                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - dX);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - dY);
            }
        }
        void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            scrollViewer.Cursor = Cursors.Arrow;
            scrollViewer.ReleaseMouseCapture();
            _lastDragPoint = null;
        }


        void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            _lastMousePositionOnTarget = Mouse.GetPosition(scrollViewer);

            if (e.Delta > 0)
            {
                slider.Value += 0.5;
            }
            if (e.Delta < 0)
            {
                slider.Value -= 0.5;
            }

            e.Handled = true;
        }

        void OnSliderValueChanged(object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue == 1)
            {
                scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }
            else
            {
                scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            }
            scaleTransform.ScaleX = e.NewValue;
            scaleTransform.ScaleY = e.NewValue;

            var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
            _lastCenterPositionOnTarget = scrollViewer.TranslatePoint(centerOfViewport, Grid);
        }

        void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                Point? targetBefore = null;
                Point? targetNow = null;

                if (!_lastMousePositionOnTarget.HasValue)
                {
                    if (_lastCenterPositionOnTarget.HasValue)
                    {
                        var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2,
                                                         scrollViewer.ViewportHeight / 2);
                        Point centerOfTargetNow =
                              scrollViewer.TranslatePoint(centerOfViewport, Grid);

                        targetBefore = _lastCenterPositionOnTarget;
                        targetNow = centerOfTargetNow;
                    }
                }
                else
                {
                    targetBefore = _lastMousePositionOnTarget;
                    targetNow = Mouse.GetPosition(Grid);

                    _lastMousePositionOnTarget = null;
                }

                if (targetBefore.HasValue)
                {
                    double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                    double multiplicatorX = e.ExtentWidth / Grid.ActualWidth;
                    double multiplicatorY = e.ExtentHeight / Grid.ActualHeight;

                    double newOffsetX = scrollViewer.HorizontalOffset -
                                        dXInTargetPixels * multiplicatorX;
                    double newOffsetY = scrollViewer.VerticalOffset -
                                        dYInTargetPixels * multiplicatorY;

                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                    {
                        return;
                    }

                    scrollViewer.ScrollToHorizontalOffset(newOffsetX);
                    scrollViewer.ScrollToVerticalOffset(newOffsetY);
                }
            }
        }

        public byte[] ImageContent
        {
            get { return (byte[])GetValue(ImageContentProperty); }
            set { SetValue(ImageContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageContentProperty =
            DependencyProperty.Register("ImageContent", typeof(byte[]), typeof(ZoomImage), new PropertyMetadata(null));

        
    }
}
