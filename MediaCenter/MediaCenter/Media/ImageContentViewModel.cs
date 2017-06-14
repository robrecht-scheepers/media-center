using MediaCenter.MVVM;

namespace MediaCenter.Media
{
    public class ImageContentViewModel : MediaContentViewModel
    {
        private ImageContent _imageContent;

        public ImageContentViewModel(ImageContent imageContent) : base(imageContent)
        {
            _imageContent = imageContent;
        }

        public ImageContent ImageContent => (ImageContent) MediaContent; 

        private RelayCommand _rotateClockwiseCommand;
        public RelayCommand RotateClockwiseCommand
            => _rotateClockwiseCommand ?? (_rotateClockwiseCommand = new RelayCommand(RotateClockwise));
        private void RotateClockwise()
        {
            ImageContent.RotateImage(RotationDirection.Clockwise);
        }

        private RelayCommand _rotateCounterclockwiseCommand;
        public RelayCommand RotateCounterclockwiseCommand
            => _rotateCounterclockwiseCommand ?? (_rotateCounterclockwiseCommand = new RelayCommand(RotateCounterClockwise));
        private void RotateCounterClockwise()
        {
            ImageContent.RotateImage(RotationDirection.Counterclockwise);
        }
    }
}
