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
        private double _previousHorizontalOffset;
        private double _previousVerticalOffset;

        public ZoomImage()
        {
            InitializeComponent();

            ScaleTransform.CenterX = 0.5;
            ScaleTransform.CenterY = 0.5;

            ScrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            ScrollViewer.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            ScrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;
            ScrollViewer.MouseMove += OnMouseMove;
            ScrollViewer.MouseLeftButtonUp += OnMouseLeftButtonUp;
            ScrollViewer.MouseDoubleClick += OnMouseDoubleClick;
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                AdjustZoom(Math.Min(_zoomLevel + 1, MaxZoom), e.GetPosition(ScrollViewer));
            }
            else if (e.Delta < 0)
            {
                AdjustZoom(Math.Max(1, _zoomLevel - 1), e.GetPosition(ScrollViewer));
            }

            e.Handled = true;
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AdjustZoom(_zoomLevel > 1 ? 1 : DefaultZoom, e.GetPosition(ScrollViewer));
        }

        private void AdjustZoom(int newZoomLevel, Point position = default(Point))
        {
            if(newZoomLevel == _zoomLevel)
                return;

            _previousZoomLevel = _zoomLevel;
            _previousHorizontalOffset = ScrollViewer.HorizontalOffset;
            _previousVerticalOffset = ScrollViewer.VerticalOffset;

            _zoomLevel = newZoomLevel;
            _zoomMousePoint = position;

            // handle the zooming here, the panning will be done in the scrollChanged event handler 
            ScaleTransform.ScaleX = newZoomLevel;
            ScaleTransform.ScaleY = newZoomLevel;
        }

        void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if(_zoomLevel == 1)
                return;

            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                var scaleFactor = ((double)_zoomLevel)/((double)_previousZoomLevel);
                var targetPoint = new Point((_previousHorizontalOffset + _zoomMousePoint.X)*scaleFactor,
                    (_previousVerticalOffset + _zoomMousePoint.Y) * scaleFactor);
                
                var offsetX = Math.Max(targetPoint.X - _zoomMousePoint.X, 0);
                var offsetY = Math.Max(targetPoint.Y - _zoomMousePoint.Y, 0);

                ScrollViewer.ScrollToHorizontalOffset(offsetX);
                ScrollViewer.ScrollToVerticalOffset(offsetY);
            }
        }

        void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_zoomLevel == 1)
                return;

            var mousePos = e.GetPosition(ScrollViewer);
            ScrollViewer.Cursor = Cursors.SizeAll;
            _lastDragPoint = mousePos;
            Mouse.Capture(ScrollViewer);
        }

        void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_lastDragPoint.HasValue) return;

            var posNow = e.GetPosition(ScrollViewer);
            var dX = posNow.X - _lastDragPoint.Value.X;
            var dY = posNow.Y - _lastDragPoint.Value.Y;

            _lastDragPoint = posNow;

            ScrollViewer.ScrollToHorizontalOffset(ScrollViewer.HorizontalOffset - dX);
            ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset - dY);
        }
        void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ScrollViewer.Cursor = Cursors.Arrow;
            ScrollViewer.ReleaseMouseCapture();
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
        
        private void Reset()
        {
            AdjustZoom(1);
        }
    }
}
