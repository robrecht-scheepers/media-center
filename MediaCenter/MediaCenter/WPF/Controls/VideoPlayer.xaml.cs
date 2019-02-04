using MediaCenter.Media;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using Vlc.DotNet.Core;

namespace MediaCenter.WPF.Controls
{
    /// <summary>
    /// Interaction logic for VideoPlayer.xaml
    /// </summary>
    public partial class VideoPlayer : UserControl
    {
        private bool _isDragging = false;
        private Uri _currentUri = default(Uri);
        private DispatcherTimer _timer;

        private long _mediaLength = 0;

        private DateTime _lastSliderUpdateTimestamp;
        private double _lastPositionNotified;
        private DateTime _lastPositionNotificationTimestamp;
        private bool _newPositionAvailable;
        
        private Vlc.DotNet.Forms.VlcControl MediaPlayer => VlcWpfControl.MediaPlayer;    
        
        public VideoPlayer()
        {
            InitializeComponent();
            InitVlcPlayer();
            PlayState = PlayState.Stopped;

            _timer = new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(500) };
            _timer.Tick += TimerOnTick;
        }
        
        private void InitVlcPlayer()
        {
            var vlcLibDir = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "libvlc",
                IntPtr.Size == 4 ? "win-x86" : "win-x64"));
            var options = new string[]
            {
                
            };

            MediaPlayer.BeginInit();
            MediaPlayer.VlcLibDirectory = vlcLibDir;
            MediaPlayer.VlcMediaplayerOptions = options;
            MediaPlayer.EndInit();

            MediaPlayer.LengthChanged += MediaPlayerOnLengthChanged;
            MediaPlayer.EndReached += VlcPlayerOnEndReached;
            MediaPlayer.PositionChanged += MediaPlayerOnPositionChanged;
        }

        
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
        private bool _updatingPosition;

        private static void OnPlayStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = (VideoPlayer) d;
            me?.ApplyPlayStateChange((PlayState)e.OldValue, (PlayState)e.NewValue);
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

        public void LoadVideo(Uri videoUri)
        {
            if(_currentUri == videoUri)
                return;

            Stop();
            MediaPlayer.SetMedia(videoUri);
            _currentUri = videoUri;
        }

        public void Rotate(int angle)
        {
            // no rotation needed, as VLC is capable of reading the orientation tag of the video
        }

        #region SeekBar
        private void TimerOnTick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            // position property is updated irregularly so to make the slider smooth, use interval times when no new value is available
            double newPosition;
            if (_newPositionAvailable)
            {
                newPosition = _lastPositionNotified + now.Subtract(_lastPositionNotificationTimestamp).TotalMilliseconds/_mediaLength;
                CurrentTime.Text = TimeSpan.FromMilliseconds(MediaPlayer.Time).ToString("mm\\:ss");
                _newPositionAvailable = false;
            }
            else
            {
                var timeInterval = now.Subtract(_lastSliderUpdateTimestamp).TotalMilliseconds;
                newPosition = SeekSlider.Value + (timeInterval / _mediaLength);
            }

            _lastSliderUpdateTimestamp = now;

            Console.WriteLine($@"S;{DateTime.Now:ss.fff};{ newPosition}");

            if (!_isDragging)
            {
                _updatingPosition = true;
                SeekSlider.Value = newPosition;
                _updatingPosition = false;
            }
        }

        private void MediaPlayerOnPositionChanged(object sender, VlcMediaPlayerPositionChangedEventArgs e)
        {
            _lastPositionNotified = e.NewPosition;
            _lastPositionNotificationTimestamp = DateTime.Now;
            _newPositionAvailable = true;
            Console.WriteLine($@"P;{DateTime.Now:ss.fff};{_lastPositionNotified}");
        }


        private void MediaPlayerOnLengthChanged(object sender, VlcMediaPlayerLengthChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _mediaLength = MediaPlayer.Length;
                SeekSlider.Value = 0;
                _lastSliderUpdateTimestamp = DateTime.Now;
                CurrentTime.Text = "00:00";
                TotalTime.Text = TimeSpan.FromMilliseconds(_mediaLength).ToString("mm\\:ss");

                _lastSliderUpdateTimestamp = DateTime.Now;
                _timer.Start();
                if(StartOnLoad)
                    Play();
                
            });
        }

        private void SeekSlider_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            _isDragging = true;
        }

        private void SeekSlider_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            MediaPlayer.Position = (float)SeekSlider.Value;
            _isDragging = false;
        }
        #endregion

        #region Control
        
        private void ApplyPlayStateChange(PlayState oldPlayState, PlayState newPlayState)
        { 
            if (oldPlayState == newPlayState)
                return;

            switch (newPlayState)
            {
                case PlayState.Stopped:
                    MediaPlayer.Stop();
                    break;
                case PlayState.Paused:
                    MediaPlayer.Pause();
                    break;
                case PlayState.Playing:
                    MediaPlayer.Play();
                    break;
                case PlayState.Finished:
                    break;
            }
        }

        private void Stop()
        {
            PlayState = PlayState.Stopped;
        }

        private void Pause()
        {
            PlayState = PlayState.Paused;
        }

        private void Play()
        {
            PlayState = PlayState.Playing;
        }

        private void PlayPause_OnClick(object sender, RoutedEventArgs e)
        {
            if (PlayState == PlayState.Playing)
                Pause();
            else
                Play();
        }

        private void Stop_OnClick(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void VlcPlayerOnEndReached(object sender, VlcMediaPlayerEndReachedEventArgs e)
        {
            Dispatcher.Invoke(() => { PlayState = PlayState.Finished; });
        }
        #endregion

        private void SeekSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isDragging && !_updatingPosition)
            {
                _newPositionAvailable = false;
                MediaPlayer.Position = (float)SeekSlider.Value;
            }
        }
    }
}
