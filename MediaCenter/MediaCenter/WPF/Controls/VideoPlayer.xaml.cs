using MediaCenter.Media;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Vlc.DotNet.Core;
using Vlc.DotNet.Core.Interops.Signatures;

namespace MediaCenter.WPF.Controls
{
    /// <summary>
    /// Interaction logic for VideoPlayer.xaml
    /// </summary>
    public partial class VideoPlayer : UserControl
    {
        private bool _isDragging = false;
        private PlayState _playstateBeforeDragging;

        private Uri _currentUri = default(Uri);
        private readonly DispatcherTimer _timer;

        private long _mediaLength = 0;
        private bool _endReached = false;

        private DateTime _lastSliderUpdateTimestamp;
        private double _lastPositionNotified;
        private DateTime _lastPositionNotificationTimestamp;
        private bool _newPositionAvailable;
        private bool _updatingSeekBar;

        private Vlc.DotNet.Forms.VlcControl MediaPlayer => VlcWpfControl.MediaPlayer;

        private readonly DirectoryInfo _vlcLibDir;
        private readonly string[] _vlcOptions;
        
        public VideoPlayer()
        {
            InitializeComponent();
            
            _timer = new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(500) };
            _timer.Tick += TimerOnTick;

            _vlcLibDir = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "libvlc",
                IntPtr.Size == 4 ? "win-x86" : "win-x64"));
            _vlcOptions = new string[] { };
            InitVlcPlayer();
        }
        
        private void InitVlcPlayer()
        {
            MediaPlayer.BeginInit();
            MediaPlayer.VlcLibDirectory = _vlcLibDir;
            MediaPlayer.VlcMediaplayerOptions = _vlcOptions;
            MediaPlayer.EndInit();

            MediaPlayer.LengthChanged += MediaPlayerOnLengthChanged;
            MediaPlayer.EndReached += VlcPlayerOnEndReached;
            MediaPlayer.PositionChanged += MediaPlayerOnPositionChanged;
        }

        #region Dependency Properties

        public bool HideControls { get { return (bool)GetValue(HideControlsProperty); } set { SetValue(HideControlsProperty, value); } }
        public static readonly DependencyProperty HideControlsProperty = DependencyProperty.Register("HideControls", typeof(bool), typeof(VideoPlayer), new PropertyMetadata(false));

        public bool StartOnLoad { get { return (bool)GetValue(StartOnLoadProperty); } set { SetValue(StartOnLoadProperty, value); } }
        public static readonly DependencyProperty StartOnLoadProperty = DependencyProperty.Register("StartOnLoad", typeof(bool), typeof(VideoPlayer), new PropertyMetadata(false));
        
        public Uri VideoUri { get { return (Uri)GetValue(VideoUriProperty); } set { SetValue(VideoUriProperty, value); } }
        public static readonly DependencyProperty VideoUriProperty = DependencyProperty.Register("VideoUri", typeof(System.Uri), typeof(VideoPlayer), new PropertyMetadata(default(Uri), VideoUriChanged));

        public int Rotation { get { return (int)GetValue(RotationProperty); } set { SetValue(RotationProperty, value); } }
        public static readonly DependencyProperty RotationProperty = DependencyProperty.Register("Rotation", typeof(int), typeof(VideoPlayer), new PropertyMetadata(0, RotationChanged));
        
        public PlayState PlayState{ get { return (PlayState)GetValue(PlayStateProperty); } set { SetValue(PlayStateProperty, value); } }
        public static readonly DependencyProperty PlayStateProperty = DependencyProperty.Register("PlayState", typeof(PlayState), typeof(VideoPlayer), new PropertyMetadata(PlayState.Stopped, OnPlayStateChanged));
        

        private static void OnPlayStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = (VideoPlayer) d;
            //me?.ApplyPlayStateChange((PlayState)e.OldValue, (PlayState)e.NewValue);
        }
        private static void RotationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as VideoPlayer;
            me?.Rotate((int)e.NewValue);
        }

        private static void VideoUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as VideoPlayer;
            me?.LoadVideo((Uri)e.NewValue);
        }

        #endregion

        public void Rotate(int angle)
        {
            // no rotation needed, as VLC is capable of reading the orientation tag of the video
        }

        public void LoadVideo(Uri videoUri)
        {
            if(_currentUri == videoUri) return;
            _currentUri = videoUri;

            if (MediaPlayer.State == MediaStates.Playing)
                Task.Run(() => MediaPlayer.Stop());

            if (videoUri != null)
            {
                Task.Run(() => MediaPlayer.SetMedia(videoUri));
            }
        }
        
        private void MediaPlayerOnLengthChanged(object sender, VlcMediaPlayerLengthChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _mediaLength = (long)e.NewLength;
                _lastSliderUpdateTimestamp = DateTime.Now;

                SeekSlider.Value = 0;
                CurrentTime.Text = "00:00";
                TotalTime.Text = TimeSpan.FromMilliseconds(_mediaLength).ToString("mm\\:ss");

                if (StartOnLoad)
                    Play();
            });
        }

        private void TimerOnTick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            double newPosition;
            if (_newPositionAvailable)
            {
                newPosition = _lastPositionNotified + now.Subtract(_lastPositionNotificationTimestamp).TotalMilliseconds/_mediaLength;
                CurrentTime.Text = TimeSpan.FromMilliseconds(MediaPlayer.Time).ToString("mm\\:ss");
                _newPositionAvailable = false;
            }
            else // to make the slider smooth, when no new position is available estimate the position base on elapsed time 
            {
                var timeInterval = now.Subtract(_lastSliderUpdateTimestamp).TotalMilliseconds;
                newPosition = SeekSlider.Value + (timeInterval / _mediaLength);
            }

            _lastSliderUpdateTimestamp = now;

            if (!_isDragging)
            {
                _updatingSeekBar = true;
                SeekSlider.Value = newPosition;
                _updatingSeekBar = false;
            }
        }

        private void MediaPlayerOnPositionChanged(object sender, VlcMediaPlayerPositionChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _lastPositionNotified = e.NewPosition;
                _lastPositionNotificationTimestamp = DateTime.Now;
                _newPositionAvailable = true;
            });
        }

        private void VlcPlayerOnEndReached(object sender, VlcMediaPlayerEndReachedEventArgs e)
        {
            Stop();
            Dispatcher.Invoke(() =>
            {
                PlayState = PlayState.Finished;
            });
        }

        private void SeekSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_updatingSeekBar)
            {
                _newPositionAvailable = false;
                MediaPlayer.Position = (float)SeekSlider.Value;
            }
        }

        private void SeekSlider_DragStarted(object sender, EventArgs e)
        {
            _playstateBeforeDragging = PlayState;
            _isDragging = true;
            Pause();
        }

        private void SeekSlider_OnDragCompleted(object sender, EventArgs e)
        {
            MediaPlayer.Position = (float)SeekSlider.Value;
            _isDragging = false;

            if (_playstateBeforeDragging == PlayState.Playing)
                Play();
        }

        private void Play()
        {
            Task.Run(() => MediaPlayer.Play());
            Dispatcher.Invoke(() =>
            {
                _timer.Start();
                PlayState = PlayState.Playing;
            });
        }

        private void Stop()
        {
            Task.Run(() => MediaPlayer.Stop());
            Dispatcher.Invoke(() =>
            {
                _timer.Stop();
                SeekSlider.Value = SeekSlider.Minimum;
                CurrentTime.Text = "00:00";
                PlayState = PlayState.Stopped;
            });
        }

        private void Pause()
        {
            Task.Run(() => MediaPlayer.Pause());
            Dispatcher.Invoke(() =>
            {
                _timer.Stop();
                PlayState = PlayState.Paused;
            });
        }

        private void Play_OnClick(object sender, RoutedEventArgs e)
        {
            Play();
        }

        private void Pause_OnClick(object sender, RoutedEventArgs e)
        {
            Pause();
        }

        private void Stop_OnClick(object sender, RoutedEventArgs e)
        {
            Stop();
        }
        
    }
}
