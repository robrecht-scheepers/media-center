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
        private int _milisecondPerSliderTick = 200;
        private DispatcherTimer _timer;
        private bool _isDragging = false;
        //private bool _sliderlengthSet = false;
        private Uri _currentUri = default(Uri);

        private Vlc.DotNet.Forms.VlcControl VlcControl => VlcWpfControl.MediaPlayer;    
        private VlcMediaPlayer VlcPlayer => VlcWpfControl.MediaPlayer.VlcMediaPlayer;
        
        public VideoPlayer()
        {
            InitializeComponent();

            PlayState = PlayState.Stopped;
            _timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _timer.Tick += new EventHandler(TimerTick);
        }

        private void InitVlcPlayer()
        {
            var vlcLibDir = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "libvlc",
                IntPtr.Size == 4 ? "win-x86" : "win-x64"));
            var options = new string[]
            {
                "--file-logging", "-vvv", "--extraintf=logger", "--logfile=Logs.log"
            };

            VlcControl.BeginInit();
            VlcControl.VlcLibDirectory = vlcLibDir;
            VlcControl.VlcMediaplayerOptions = options;
            VlcControl.EndInit();

            VlcControl.MediaChanged += VlcControlOnMediaChanged;
            VlcControl.EndReached += VlcPlayerOnEndReached;
        }
        
        public bool HideControls
        {
            get { return (bool)GetValue(HideControlsProperty); }
            set { SetValue(HideControlsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HideControls.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HideControlsProperty =
            DependencyProperty.Register("HideControls", typeof(bool), typeof(VideoPlayer), new PropertyMetadata(false));
        

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

            me.ApplyPlayStateChange((PlayState)e.OldValue, (PlayState)e.NewValue);
        }

        private static void RotationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as VideoPlayer;
            if (me == null) return;

            me.Rotate((int)e.NewValue);
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
            VlcControl.SetMedia(videoUri);
            _currentUri = videoUri;
        }

        public void Rotate(int angle)
        {
            if (angle == 0)
            {
                VlcWpfControl.LayoutTransform = null;
            }
            else
            {
                VlcWpfControl.LayoutTransform = new RotateTransform(angle);
            }
        }

        #region SeekBar
        private void TimerTick(object sender, EventArgs eventArgs)
        {
            if (!_isDragging)
            {
                SeekSlider.Value = VlcPlayer.Time;
            }
            CurrentTime.Text = VlcPlayer.Time.ToString("mm\\:ss");
        }

        private void VlcControlOnMediaChanged(object sender, VlcMediaPlayerMediaChangedEventArgs e)
        {
            SeekSlider.Minimum = 0;
            SeekSlider.Value = 0;
            
            SeekSlider.Maximum = VlcControl.Length;
            CurrentTime.Text = "00:00";
            TotalTime.Text = VlcControl.Length.ToString("mm\\:ss");
            //_sliderlengthSet = true;

            _timer.Start();

            if(StartOnLoad)
                Play(); 
        }

        private void SeekSlider_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            _isDragging = true;
        }

        private void SeekSlider_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            VlcPlayer.Position = (int)SeekSlider.Value * 1000;
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
                    VlcControl.Stop();
                    break;
                case PlayState.Paused:
                    VlcControl.Pause();
                    break;
                case PlayState.Playing:
                    VlcControl.Play();
                    break;
                case PlayState.Finished:
                    // do nothing, the media element has stopped by itself. 
                    // The state is set to finished, to notify anyone who needs to know this (for instance the slide show)
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
            PlayState = PlayState.Finished; 
        }
        #endregion

        private void SeekSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(!_isDragging)
                VlcControl.Position = (int)SeekSlider.Value * 1000;
        }
    }
}
