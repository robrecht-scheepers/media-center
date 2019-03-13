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
        private const int ZoomLevel = 5;

        private Point? _lastCenterPositionOnTarget;
        private Point? _lastMousePositionOnTarget;
        private Point? _lastDragPoint;
        private Point _relativeZoomPoint;

        private bool _zoomedIn;
        private bool _zoomingIn;
        private bool _zoomingOut;

        public ZoomImage()
        {
            InitializeComponent();

            scaleTransform.CenterX = 0.5;
            scaleTransform.CenterY = 0.5;

            scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            //scrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;
            scrollViewer.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            scrollViewer.MouseMove += OnMouseMove;
            scrollViewer.MouseLeftButtonUp += OnMouseLeftButtonUp;
            scrollViewer.MouseDoubleClick += OnMouseDoubleClick;
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(_zoomedIn)
                ZoomOut(); 
            else
                ZoomIn(e.GetPosition(scrollViewer));
        }

        void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_zoomedIn)
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

        private void ZoomIn(Point position)
        {
            _zoomingIn = true;
            scaleTransform.ScaleX = ZoomLevel;
            scaleTransform.ScaleY = ZoomLevel;
            
            _relativeZoomPoint = new Point(position.X / scrollViewer.ViewportWidth, position.Y / scrollViewer.ViewportHeight);
            _zoomedIn = true;
        }

        private void ZoomOut()
        {
            scaleTransform.ScaleX = 1;
            scaleTransform.ScaleY = 1;

            _zoomedIn = false;
        }

        void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if ((e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0) && _zoomingIn)
            {
                var targetPoint = new Point(_relativeZoomPoint.X * scrollViewer.ExtentWidth, _relativeZoomPoint.Y * scrollViewer.ExtentHeight);

                var offsetX = Math.Max(targetPoint.X - scrollViewer.ViewportWidth / 2, 0);
                var offsetY = Math.Max(targetPoint.Y - scrollViewer.ViewportHeight / 2, 0);

                scrollViewer.ScrollToHorizontalOffset(offsetX);
                scrollViewer.ScrollToVerticalOffset(offsetY);
                _zoomingIn = false;
            }
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
            ZoomOut();
        }
    }
}
