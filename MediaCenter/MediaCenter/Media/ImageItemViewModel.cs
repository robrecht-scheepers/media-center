using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public MediaItem MediaItem { get; }

        public void RotateImage(RotationDirection direction)
        {
            MediaItem.Content = ImageHelper.Rotate(MediaItem.Content, direction);
            MediaItem.IsContentDirty = true;

            MediaItem.Thumbnail = ImageHelper.CreateThumbnail(MediaItem.Content, 100);
            MediaItem.IsThumbnailDirty = true;
        }

        private RelayCommand _rotateClockwiseCommand;
        public RelayCommand RotateClockwiseCommand
            => _rotateClockwiseCommand ?? (_rotateClockwiseCommand = new RelayCommand(RotateClockwise));
        private void RotateClockwise()
        {
            RotateImage(RotationDirection.Clockwise);
        }

        private RelayCommand _rotateCounterclockwiseCommand;
        public RelayCommand RotateCounterclockwiseCommand
            => _rotateCounterclockwiseCommand ?? (_rotateCounterclockwiseCommand = new RelayCommand(RotateCounterClockwise));
        private void RotateCounterClockwise()
        {
            RotateImage(RotationDirection.Counterclockwise);
        }
    }
}
