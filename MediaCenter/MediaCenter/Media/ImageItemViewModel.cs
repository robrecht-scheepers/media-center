using System;
using MediaCenter.Helpers;
using MediaCenter.MVVM;

namespace MediaCenter.Media
{
    public class ImageItemViewModel : MediaItemViewModel
    {
        public ImageItemViewModel(MediaItem item) 
        {
            if(item.MediaType != MediaType.Image)
                throw new ArgumentException($"Cannot create image view model for non-image item. Item type is {item.MediaType}.");
            MediaItem = item;
        }

        public void RotateImage(int angle)
        {
            MediaItem.Rotation = (MediaItem.Rotation + angle) % 360;
            MediaItem.IsInfoDirty = true;

            MediaItem.Thumbnail = ImageHelper.Rotate(MediaItem.Thumbnail, angle);
            MediaItem.IsThumbnailDirty = true;
        }

        private RelayCommand _rotateClockwiseCommand;
        public RelayCommand RotateClockwiseCommand
            => _rotateClockwiseCommand ?? (_rotateClockwiseCommand = new RelayCommand(RotateClockwise));
        private void RotateClockwise()
        {
            RotateImage(90);
        }

        private RelayCommand _rotateCounterclockwiseCommand;
        public RelayCommand RotateCounterclockwiseCommand
            => _rotateCounterclockwiseCommand ?? (_rotateCounterclockwiseCommand = new RelayCommand(RotateCounterClockwise));
        private void RotateCounterClockwise()
        {
            RotateImage(270);
        }
    }
}
