using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace VideoPlayerPOC
{
    public class VideoControlBehavior : Behavior<VideoPlayer>
    {
        public PlayState RequestedPlayerState
        {
            get => (PlayState) GetValue(RequestedPlayStateProperty);
            set => SetValue(RequestedPlayStateProperty, value);
        }

        public static readonly DependencyProperty RequestedPlayStateProperty =
            DependencyProperty.Register("RequestedPlayerState", typeof(PlayState), typeof(VideoControlBehavior),
                new PropertyMetadata(null, RequestedPlayStateChanged));

        private static void RequestedPlayStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as VideoControlBehavior;
            var mediaElement =me?.AssociatedObject.MediaElement;

            switch ((PlayState)e.NewValue)
            {
                case PlayState.Play:
                    mediaElement?.Play();
                    break;
                case PlayState.Paused:
                    mediaElement?.Pause();
                    break;
                case PlayState.Stopped:
                    mediaElement?.Stop();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }
    }

}
