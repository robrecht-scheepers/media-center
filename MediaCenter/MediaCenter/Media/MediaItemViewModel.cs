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

            MediaItem = item;
            if (MediaItem.MediaType == MediaType.Image)
            {
                ContentBytes = await _repository.GetFullImage(MediaItem);
                ContentUri = null;
            }
            else // video
            {
                ContentBytes = null;
                ContentUri = _repository.GetContentUri(MediaItem);
            }
        }
    }
}
