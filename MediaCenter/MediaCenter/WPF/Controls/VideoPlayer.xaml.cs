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
        private bool _isDragging;
        private PlayState _playStateBeforeDragging;

        private Uri _currentUri;
        private readonly DispatcherTimer _timer;

        private long _mediaLength;
        private DateTime _timeLastSeekBarUpdate;
        private double _lastNotifiedPosition;
        private DateTime _timeLastNotifiedPosition;
        private bool _newPositionAvailable;

        private bool _seekBarUpdateInProgress;

        private Vlc.DotNet.Forms.VlcControl MediaPlayer => VlcWpfControl.MediaPlayer; // shortcut for code readability
        
        public VideoPlayer()
        {
            InitializeComponent();
            
            _timer = new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(500) };
            _timer.Tick += TimerOnTick;

            InitVlcPlayer();
        }
        
        private void InitVlcPlayer()
        {
            MediaPlayer.BeginInit();
            MediaPlayer.VlcLibDirectory = new DirectoryInfo(
                Path.Combine(Directory.GetCurrentDirectory(), "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
            MediaPlayer.VlcMediaplayerOptions = new string[] { };
            MediaPlayer.EndInit();

            MediaPlayer.MediaChanged += MediaPlayerOnMediaChanged;
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
        public static readonly DependencyProperty PlayStateProperty = DependencyProperty.Register("PlayState", typeof(PlayState), typeof(VideoPlayer), new PropertyMetadata(PlayState.Stopped));
        
        private static void RotationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // no rotation needed, as VLC is capable of reading the orientation tag of the video
        }

        private static void VideoUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as VideoPlayer;
            me?.LoadVideo((Uri)e.NewValue);
        }

        #endregion

        public void LoadVideo(Uri videoUri)
        {
            if(_currentUri == videoUri) return;
            _currentUri = videoUri;
            
            if (MediaPlayer.State == MediaStates.Playing)
                    MediaPlayer.Stop();

            if (videoUri != null)
            {
                Task.Run(() => MediaPlayer.SetMedia(videoUri));
            }
        }
        private void MediaPlayerOnMediaChanged(object sender, VlcMediaPlayerMediaChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                SetSeekBarPosition(SeekBar.Minimum);
                if (StartOnLoad)
                    Play();
            });
        }

        private void MediaPlayerOnLengthChanged(object sender, VlcMediaPlayerLengthChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _mediaLength = (long)e.NewLength/10000; // e.NewLength expressed in ticks (0,1 microseconds)
                _timeLastSeekBarUpdate = DateTime.Now;
                SetSeekBarPosition(SeekBar.Minimum);
                CurrentTime.Text = "00:00";
                TotalTime.Text = TimeSpan.FromMilliseconds(_mediaLength).ToString("mm\\:ss");
            });
        }
        private void MediaPlayerOnPositionChanged(object sender, VlcMediaPlayerPositionChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _lastNotifiedPosition = e.NewPosition;
                _timeLastNotifiedPosition = DateTime.Now;
                _newPositionAvailable = true;
            });
        }

        private void TimerOnTick(object sender, EventArgs e)
        {
            if(_mediaLength == 0) // media length not known yet 
                return;
            if (_isDragging)
                return;
            
            var now = DateTime.Now;
            double newPosition;
            if (_newPositionAvailable)
            {
                newPosition = _lastNotifiedPosition +
                              now.Subtract(_timeLastNotifiedPosition).TotalMilliseconds / _mediaLength;
                CurrentTime.Text = TimeSpan.FromMilliseconds(MediaPlayer.Time).ToString("mm\\:ss");
                _newPositionAvailable = false;
            }
            else // to make the slider move smoothly, when no new position is available we estimate the position base on elapsed time 
            {
                var timeInterval = now.Subtract(_timeLastSeekBarUpdate).TotalMilliseconds;
                newPosition = SeekBar.Value + (timeInterval / _mediaLength);
            }

            _timeLastSeekBarUpdate = now;
            SetSeekBarPosition(newPosition);
        }

        private void VlcPlayerOnEndReached(object sender, VlcMediaPlayerEndReachedEventArgs e)
        {
            Stop();
            Dispatcher.Invoke(() =>
            {
                PlayState = PlayState.Finished;
            });
        }

        private void SeekSlider_DragStarted(object sender, EventArgs e)
        {
            _playStateBeforeDragging = PlayState;
            _isDragging = true;
            Pause();
        }

        private void SeekSlider_OnDragCompleted(object sender, EventArgs e)
        {
            _isDragging = false;

            if (_playStateBeforeDragging == PlayState.Playing)
                Play();
        }

        private void SeekSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_seekBarUpdateInProgress)
            {
                _newPositionAvailable = false;
                MediaPlayer.Position = (float)SeekBar.Value;
            }
        }

        private void SetSeekBarPosition(double newPosition)
        {
            _seekBarUpdateInProgress = true;
            SeekBar.Value = newPosition;
            _seekBarUpdateInProgress = false;
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
                SetSeekBarPosition(SeekBar.Minimum);
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
