using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MediaCenter.WPF.Controls
{
    /// <summary>
    /// Interaction logic for ZoomImage.xaml
    /// </summary>
    public partial class ZoomImage : UserControl
    {
        private const int DefaultZoom = 4;
        private const int MaxZoom = 10;

        private Point? _lastDragPoint;
        private Point _zoomMousePoint;


        private int _zoomLevel = 1;
        private int _previousZoomLevel = 1;

        public ZoomImage()
        {
            InitializeComponent();

            scaleTransform.CenterX = 0.5;
            scaleTransform.CenterY = 0.5;

            scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            scrollViewer.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            //scrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;
            scrollViewer.MouseMove += OnMouseMove;
            scrollViewer.MouseLeftButtonUp += OnMouseLeftButtonUp;
            scrollViewer.MouseDoubleClick += OnMouseDoubleClick;
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                AdjustZoom(Math.Min(_zoomLevel + 1, MaxZoom), e.GetPosition(scrollViewer));
            }
            else if (e.Delta < 0)
            {
                AdjustZoom(Math.Max(1, _zoomLevel - 1), e.GetPosition(scrollViewer));
            }

            e.Handled = true;
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AdjustZoom(_zoomLevel > 1 ? 1 : DefaultZoom, e.GetPosition(scrollViewer));
        }

        private void AdjustZoom(int newZoomLevel, Point position = default(Point))
        {
            if(newZoomLevel == _zoomLevel)
                return;

            _previousZoomLevel = _zoomLevel;
            _zoomLevel = newZoomLevel;
            _zoomMousePoint = position;

            // handle the zooming here, the panning will be done in the scrollChanged event handler 
            scaleTransform.ScaleX = newZoomLevel;
            scaleTransform.ScaleY = newZoomLevel;
        }

        void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if(_zoomLevel == 1)
                return;

            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                var scaleFactor = ((double)_zoomLevel)/((double)_previousZoomLevel);
                var targetPoint = new Point((scrollViewer.HorizontalOffset + _zoomMousePoint.X)*scaleFactor,
                    (scrollViewer.VerticalOffset + _zoomMousePoint.Y) * scaleFactor);
                
                var offsetX = Math.Max(targetPoint.X - _zoomMousePoint.X, 0);
                var offsetY = Math.Max(targetPoint.Y - _zoomMousePoint.Y, 0);

                scrollViewer.ScrollToHorizontalOffset(offsetX);
                scrollViewer.ScrollToVerticalOffset(offsetY);
            }
        }

        void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_zoomLevel == 1)
                return;

            var mousePos = e.GetPosition(scrollViewer);
            scrollViewer.Cursor = Cursors.SizeAll;
            _lastDragPoint = mousePos;
            Mouse.Capture(scrollViewer);
        }

        void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_lastDragPoint.HasValue) return;

            var posNow = e.GetPosition(scrollViewer);
            var dX = posNow.X - _lastDragPoint.Value.X;
            var dY = posNow.Y - _lastDragPoint.Value.Y;

            _lastDragPoint = posNow;

            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - dX);
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - dY);
        }
        void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            scrollViewer.Cursor = Cursors.Arrow;
            scrollViewer.ReleaseMouseCapture();
            _lastDragPoint = null;
        }

        public byte[] ImageContent
        {
            get => (byte[])GetValue(ImageContentProperty);
            set => SetValue(ImageContentProperty, value);
        }

        // Using a DependencyProperty as the backing store for ImageContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageContentProperty =
            DependencyProperty.Register("ImageContent", typeof(byte[]), typeof(ZoomImage), new PropertyMetadata(null, ImageContentChanged));

        private static void ImageContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = (ZoomImage) d;
            me.Reset();
        }

        public int Rotation
        {
            get => (int) GetValue(RotationProperty);
            set => SetValue(RotationProperty, value);
        }
        public static readonly DependencyProperty RotationProperty = DependencyProperty.Register("Rotation", typeof(int), typeof(ZoomImage), new PropertyMetadata(0));
        


        private void Reset()
        {
            AdjustZoom(1);
        }
    }
}
