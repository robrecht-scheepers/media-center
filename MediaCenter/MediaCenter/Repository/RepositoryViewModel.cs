using System;
using System.Linq;
using MediaCenter.Media;
using MediaCenter.MVVM;

namespace MediaCenter.Repository
{
    public class RepositoryViewModel : PropertyChangedNotifier
    {
        private readonly IRepository _repository;
        private int _imageItemCount;
        private int _videoItemCount;
        private int _itemCount;
        private string _statusMessage;

        public RepositoryViewModel(IRepository repository)
        {
            _repository = repository;
            _repository.CollectionChanged += RepositoryOnChanged;
            _repository.StatusChanged += RepositoryOnStatusChanged;
            StatusMessage = _repository.StatusMessage;
            UpdateRepositoryValues();
        }

        private void RepositoryOnStatusChanged(object sender, EventArgs e)
        {
            StatusMessage = _repository.StatusMessage;
        }

        private void RepositoryOnChanged(object sender, EventArgs eventArgs)
        {
            UpdateRepositoryValues();
        }

        private void UpdateRepositoryValues()
        {
            //ImageItemCount = _repository.Catalog.Count(x => x.MediaType == MediaType.Image);
            //VideoItemCount = _repository.Catalog.Count(x => x.MediaType == MediaType.Video);
            //ItemCount = ImageItemCount + VideoItemCount;            
        }

        public int ItemCount
        {
            get { return _itemCount; }
            set { SetValue(ref _itemCount, value); }
        }
        public int ImageItemCount
        {
            get { return _imageItemCount; }
            set { SetValue(ref _imageItemCount, value); }
        }

        public int VideoItemCount
        {
            get { return _videoItemCount; }
            set { SetValue(ref _videoItemCount, value); }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            set { SetValue(ref _statusMessage, value); }
        }
        
        public string Location => _repository.Location?.LocalPath;
    }
}
