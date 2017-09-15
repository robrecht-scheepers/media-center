using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
                new PropertyMetadata(PlayState.Stopped, RequestedPlayStateChanged));

        public string VideoFilePath
        {
            get => (string) GetValue(VideoFilePathProperty);
            set => SetValue(VideoFilePathProperty, value);
        }

        public static readonly DependencyProperty VideoFilePathProperty = DependencyProperty.Register("VideoFilePath",
            typeof(string), typeof(VideoControlBehavior), new PropertyMetadata(null, VideoFilePathChanged
            ));

        private static void VideoFilePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as VideoControlBehavior;
            var mediaElement = me?.AssociatedObject.MediaElement;
            if (mediaElement == null)
            {
                Debug.WriteLine("MediaElement not found");
                return;
            }

            mediaElement?.Stop();
            if(!string.IsNullOrEmpty(me.VideoFilePath))
                mediaElement.Source = FilePathToUri(me.VideoFilePath);
        }

        private static Uri FilePathToUri(string filePath)
        {
            return new System.Uri(filePath);
        }

        private static void RequestedPlayStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as VideoControlBehavior;
            var mediaElement =me?.AssociatedObject.MediaElement;
            if(mediaElement == null)
                return;
            
            switch ((PlayState)e.NewValue)
            {
                case PlayState.Play:
                    mediaElement.Play();
                    break;
                case PlayState.Paused:
                    mediaElement.Pause();
                    break;
                case PlayState.Stopped:
                    mediaElement.Stop();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }
    }

}
