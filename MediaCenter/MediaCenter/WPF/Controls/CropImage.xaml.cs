using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MediaCenter.Media;

namespace MediaCenter.WPF.Controls
{
    /// <summary>
    /// Interaction logic for CropImage.xaml
    /// </summary>
    public partial class CropImage : UserControl
    {
        public CropImage()
        {
            InitializeComponent();
            InitializeElements();
        }

        public static readonly DependencyProperty ImageContentProperty = DependencyProperty.Register(
            "ImageContent", typeof(byte[]), typeof(CropImage), new PropertyMetadata(null, ImageContentChanged));
        public byte[] ImageContent
        {
            get => (byte[]) GetValue(ImageContentProperty);
            set => SetValue(ImageContentProperty, value);
        }
        private static void ImageContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(!(d is CropImage me)) return;

            me.InitializeCanvas();
        }

        private const int MinFovSize = 50;
        private double _fovLeft, _fovTop, _fovRight, _fovBottom;

        private Shape _movingElement = null;
        private List<Shape> _leftElements;
        private List<Shape> _topElements;
        private List<Shape> _rightElements;
        private List<Shape> _bottomElements;
        private List<Shape> _horizontalEdges;
        private List<Shape> _verticalEdges;
        private RectangleGeometry _overlayClip;
        private bool _updating;

        public byte[] ImageSource { get; set; }

        private void InitializeElements()
        {
            _leftElements = new List<Shape> { BottomLeft, Left, TopLeft };
            _topElements = new List<Shape> { TopLeft, Top, TopRight };
            _rightElements = new List<Shape> { TopRight, Right, BottomRight };
            _bottomElements = new List<Shape> { BottomRight, Bottom, BottomLeft };
            _horizontalEdges = new List<Shape> { Top, Bottom };
            _verticalEdges = new List<Shape> { Left, Right };

            Left.MouseLeftButtonDown += ElementStartMove;
            TopLeft.MouseLeftButtonDown += ElementStartMove;
            Top.MouseLeftButtonDown += ElementStartMove;
            TopRight.MouseLeftButtonDown += ElementStartMove;
            Right.MouseLeftButtonDown += ElementStartMove;
            BottomLeft.MouseLeftButtonDown += ElementStartMove;
            Bottom.MouseLeftButtonDown += ElementStartMove;
            BottomRight.MouseLeftButtonDown += ElementStartMove;
            Canvas.MouseMove += ElementMove;
            Canvas.MouseUp += ElementEndMove;

            Canvas.Loaded += (s, a) => InitializeCanvas();
            Canvas.SizeChanged += (s, a) => InitializeCanvas();
        }

        private void InitializeCanvas()
        {
            var crop = Crop ?? Crop.FullImage();
            _fovLeft = crop.X * Canvas.ActualWidth;
            _fovTop = crop.Y * Canvas.ActualHeight;
            _fovRight = (crop.X + crop.Width) * Canvas.ActualWidth;
            _fovBottom = (crop.Y + crop.Height) * Canvas.ActualHeight;

            Overlay.Clip = new CombinedGeometry
            {
                GeometryCombineMode = GeometryCombineMode.Exclude,
                Geometry1 = new RectangleGeometry(new Rect(0, 0, Canvas.ActualWidth, Canvas.ActualHeight)),
                Geometry2 = _overlayClip = new RectangleGeometry(new Rect(_fovLeft, _fovTop, _fovRight - _fovLeft, _fovBottom - _fovTop))
            };

            SetFovLeft(_fovLeft);
            SetFovTop(_fovTop);
            SetFovRight(_fovRight);
            SetFovBottom(_fovBottom);
        }

        private void ElementStartMove(object sender, MouseButtonEventArgs e)
        {
            _movingElement = (Shape)sender;
            Canvas.CaptureMouse();
        }

        private void ElementEndMove(object sender, MouseButtonEventArgs e)
        {
            Canvas.ReleaseMouseCapture();
            _movingElement = null;
        }

        private void ElementMove(object sender, MouseEventArgs e)
        {
            if (_movingElement == null)
                return;

            var newPosition = e.GetPosition(Canvas);

            if (_leftElements.Contains(_movingElement))
            {
                SetFovLeft(newPosition.X);
            }
            if (_topElements.Contains(_movingElement))
            {
                SetFovTop(newPosition.Y);
            }
            if (_rightElements.Contains(_movingElement))
            {
                SetFovRight(newPosition.X);
            }
            if (_bottomElements.Contains(_movingElement))
            {
                SetFovBottom(newPosition.Y);
            }
            UpdateCrop();
        }

        private void SetFovLeft(double pos)
        {
            _fovLeft = Math.Min(Math.Max(pos, 0), _fovRight - MinFovSize);
            foreach (var element in _leftElements)
            {
                Canvas.SetLeft(element, _fovLeft - element.ActualWidth / 2);
            }
            foreach (var element in _horizontalEdges)
            {
                Canvas.SetLeft(element, (_fovLeft + _fovRight) / 2 - element.ActualWidth / 2);
            }

            UpdateFovClip();
        }

        private void SetFovTop(double pos)
        {
            _fovTop = Math.Min(Math.Max(pos, 0), _fovBottom - MinFovSize);
            foreach (var element in _topElements)
            {
                Canvas.SetTop(element, _fovTop - element.ActualHeight / 2);
            }
            foreach (var element in _verticalEdges)
            {
                Canvas.SetTop(element, (_fovTop + _fovBottom) / 2 - element.ActualHeight / 2);
            }

            UpdateFovClip();
        }

        private void SetFovRight(double pos)
        {
            _fovRight = Math.Min(Math.Max(pos, _fovLeft + MinFovSize), Canvas.ActualWidth);
            foreach (var element in _rightElements)
            {
                Canvas.SetLeft(element, _fovRight - element.ActualWidth / 2);
            }
            foreach (var element in _horizontalEdges)
            {
                Canvas.SetLeft(element, (_fovLeft + _fovRight) / 2 - element.ActualWidth / 2);
            }

            UpdateFovClip();
        }

        private void SetFovBottom(double pos)
        {
            _fovBottom = Math.Min(Math.Max(pos, _fovTop + MinFovSize), Canvas.ActualHeight);
            foreach (var element in _bottomElements)
            {
                Canvas.SetTop(element, _fovBottom - element.ActualHeight / 2);
            }
            foreach (var element in _verticalEdges)
            {
                Canvas.SetTop(element, (_fovTop + _fovBottom) / 2 - element.ActualHeight / 2);
            }

            UpdateFovClip();
        }

        private void UpdateFovClip()
        {
            _overlayClip.Rect = new Rect(_fovLeft, _fovTop, _fovRight - _fovLeft, _fovBottom - _fovTop);
        }

        public static readonly DependencyProperty CropProperty = DependencyProperty.Register(
            "Crop", typeof(Crop), typeof(CropImage), new PropertyMetadata(Crop.FullImage(), CropChanged));

        

        public Crop Crop
        {
            get => (Crop) GetValue(CropProperty);
            set => SetValue(CropProperty, value);
        }

        private static void CropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = (CropImage) d;
            if(me._updating)
                return;

            me.InitializeCanvas();
        }

        private void UpdateCrop()
        {
            var width = Canvas.ActualWidth;
            var height = Canvas.ActualHeight;

            _updating = true;
            Crop = new Crop(_fovLeft/width, _fovTop/height, (_fovRight - _fovLeft)/width, (_fovBottom - _fovTop)/height);
            _updating = false;
        }


    }
}
