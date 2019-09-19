using System;
using System.Threading.Tasks;
using MediaCenter.MVVM;
using MediaCenter.Repository;

namespace MediaCenter.Media
{
    public class MediaItemViewModel : PropertyChangedNotifier
    {
        private readonly IRepository _repository;
        private Uri _contentUri;
        private byte[] _contentBytes;
        private MediaItem _mediaItem;
        private AsyncRelayCommand _rotateClockwiseCommand;
        private AsyncRelayCommand _rotateCounterclockwiseCommand;
        private PlayState _videoPlayState;
        private AsyncRelayCommand _startCropCommand;
        private bool _isInCropMode;
        private Crop _crop;
        
        public event EventHandler VideoPlayFinished;
        
        public MediaItemViewModel(IRepository repository)
        {
            _repository = repository;
        }

        public MediaItem MediaItem
        {
            get => _mediaItem;
            set => SetValue(ref _mediaItem, value);
        }

        public Uri ContentUri
        {
            get => _contentUri;
            set => SetValue(ref _contentUri, value);
        }

        public byte[] ContentBytes
        {
            get => _contentBytes;
            set => SetValue(ref _contentBytes, value);
        }

        public PlayState VideoPlayState
        {
            get => _videoPlayState;
            set => SetValue(ref _videoPlayState, value, PlayStateChanged);
        }

        public bool IsInCropMode
        {
            get => _isInCropMode;
            set => SetValue(ref _isInCropMode, value);
        }

        public Crop Crop
        {
            get => _crop;
            set => SetValue(ref _crop, value);
        }

        public async Task Load(MediaItem item)
        {
            if(MediaItem == item)
                return;

            if (item == null) //  clear
            {
                ContentBytes = null;
                ContentUri = null;
                MediaItem = null;
                return;
            }

            // fetch the content before assigning the MediaItem property, to avoid the new rotation to be applied to the old content
            if (item.MediaType == MediaType.Image)
            {
                ContentBytes = await _repository.GetFullImage(item);
                ContentUri = null;
            }
            else // video
            {
                ContentBytes = null;
                ContentUri = _repository.GetContentUri(item);
            }
            MediaItem = item;
        }

        public async Task Rotate(int angle)
        {
            MediaItem.Rotation = (MediaItem.Rotation + angle) % 360;
            await _repository.SaveItem(MediaItem);
        }

        public AsyncRelayCommand RotateClockwiseCommand
            => _rotateClockwiseCommand ?? (_rotateClockwiseCommand = new AsyncRelayCommand(RotateClockwise, CanExecuteRotate));
        private async Task RotateClockwise()
        {
            await Rotate(90);
        }

        public AsyncRelayCommand RotateCounterclockwiseCommand
            => _rotateCounterclockwiseCommand ?? (_rotateCounterclockwiseCommand = new AsyncRelayCommand(RotateCounterClockwise, CanExecuteRotate));
        private async Task RotateCounterClockwise()
        {
            await Rotate(270);
        }

        private bool CanExecuteRotate()
        {
            return MediaItem != null;
        }

        public AsyncRelayCommand StartCropCommand => _startCropCommand ?? (_startCropCommand = new AsyncRelayCommand(StartCrop));

        private async Task StartCrop()
        {
            // if item is already edited, get the original image
            if (MediaItem.Crop != null)
            {
                ContentBytes = await _repository.GetOriginalFullImage(MediaItem);
                Crop = MediaItem.Crop.Clone();
            }
            else
            {
                Crop = Crop.FullImage();
            }

            IsInCropMode = true;
        }

        private void PlayStateChanged()
        {
            if(VideoPlayState == PlayState.Finished)
                VideoPlayFinished?.Invoke(this,EventArgs.Empty);
        }



    }
}
