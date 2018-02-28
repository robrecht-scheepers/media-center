using System;

namespace MediaCenter.Media
{
    public class VideoItemViewModel : MediaItemViewModel
    {
        private PlayState _videoPlayState;
        private PlayState _previousPlayState = PlayState.Stopped;

        public VideoItemViewModel(MediaItem item)
        {
            if (item.MediaType != MediaType.Video)
                throw new ArgumentException($"Cannot create video view model for non-video item. Item type is {item.MediaType}.");
            MediaItem = item;
        }
        
        public PlayState VideoPlayState
        {
            get { return _videoPlayState; }
            set { SetValue(ref _videoPlayState, value, PlayStateChanged); }
        }

        private void PlayStateChanged()
        {
            if(VideoPlayState == PlayState.Finished)
                VideoPlayFinished?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler VideoPlayFinished;
    }

    
}
