using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.Helpers;
using MediaCenter.MVVM;

namespace MediaCenter.Media
{
    public class ImageItem : MediaItem
    {
        public ImageItem(string name) : base(name, MediaType.Image)
        {
        }

        public void RotateImage(RotationDirection direction)
        {
            Content = ImageHelper.Rotate(Content, direction);
            IsContentDirty = true;

            Thumbnail = ImageHelper.CreateThumbnail(Content);
            IsThumbnailDirty = true;
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
