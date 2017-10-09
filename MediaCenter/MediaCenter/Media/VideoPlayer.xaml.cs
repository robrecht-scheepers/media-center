﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MediaCenter.Media
{
    /// <summary>
    /// Interaction logic for VideoPlayer.xaml
    /// </summary>
    public partial class VideoPlayer : UserControl
    {
        private enum PlayState { Playing, Paused, Stopped }

        private int _milisecondPerSliderTick = 100;
        private DispatcherTimer _timer;
        private bool _isDragging = false;
        private bool _sliderlengthSet = false;
        public VideoPlayer()
        {
            InitializeComponent();
            _timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _timer.Tick += new EventHandler(TimerTick);

            if(VideoUri != default(Uri))
                LoadVideo(VideoUri);
        }

        public Uri VideoUri { get { return (Uri)GetValue(VideoUriProperty); } set { SetValue(VideoUriProperty, value); } }
        public static readonly DependencyProperty VideoUriProperty = DependencyProperty.Register("VideoUri", typeof(System.Uri), typeof(VideoPlayer), new PropertyMetadata(default(Uri), VideoUriChanged));

        private static void VideoUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as VideoPlayer;
            if (me == null) return;

            me.LoadVideo((Uri)e.NewValue);
        }

        public void LoadVideo(Uri videoUri)
        {
            MediaElement.Stop();
            MediaElement.Source = videoUri;
        }


        #region SeekBar
        private void TimerTick(object sender, EventArgs eventArgs)
        {
            if (!_sliderlengthSet && MediaElement.NaturalDuration.HasTimeSpan)
            {
                SeekSlider.Maximum = MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds / _milisecondPerSliderTick;
                _sliderlengthSet = true;
            }
            if (!_isDragging)
            {
                SeekSlider.Value = MediaElement.Position.TotalMilliseconds / _milisecondPerSliderTick;
            }
        }

        private void MediaElement_OnMediaOpened(object sender, RoutedEventArgs e)
        {
            SeekSlider.Minimum = 0;
            SeekSlider.Value = 0;

            if (MediaElement.NaturalDuration.HasTimeSpan)
            {
                SeekSlider.Maximum = MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds / _milisecondPerSliderTick;
                _sliderlengthSet = true;
            }
            else
            {
                SeekSlider.Maximum = 0;
                _sliderlengthSet = false;
            }

            _timer.Start();
        }

        private void SeekSlider_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            _isDragging = true;
        }

        private void SeekSlider_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            MediaElement.Position = TimeSpan.FromMilliseconds(SeekSlider.Value * _milisecondPerSliderTick);
            _isDragging = false;
        }
        #endregion

        #region Control

        private PlayState _playState;

        private void ApplyPlayState(PlayState state)
        {
            if (_playState == state)
                return;

            switch (state)
            {
                case PlayState.Stopped:
                    _playState = PlayState.Stopped;
                    MediaElement.Stop();
                    break;
                case PlayState.Paused:
                    _playState = PlayState.Paused;
                    MediaElement.Pause();
                    break;
                case PlayState.Playing:
                    _playState = PlayState.Playing;
                    MediaElement.Play();
                    break;
            }
        }

        private void Stop()
        {
            ApplyPlayState(PlayState.Stopped);
        }

        private void Pause()
        {
            ApplyPlayState(PlayState.Paused);
        }

        private void Play()
        {
            ApplyPlayState(PlayState.Playing);
        }

        private void PlayPause_OnClick(object sender, RoutedEventArgs e)
        {
            if (_playState == PlayState.Playing)
                Pause();
            else
                Play();
        }

        private void Stop_OnClick(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void MediaElement_OnMediaEnded(object sender, RoutedEventArgs e)
        {
            Stop();
        }
        #endregion
    }
}
