using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.Helpers;
using MediaCenter.Media;
using MediaCenter.MVVM;

namespace MediaCenter.Repository
{
    public class RepositoryViewModel : PropertyChangedNotifier
    {
        private IRepository _repository;
        private int _imageItemCount;
        private int _videoItemCount;
        private int _size;

        public RepositoryViewModel(IRepository repository)
        {
            _repository = repository;
            _repository.Changed += RepositoryOnChanged;
            UpdateRepositoryValues();
        }

        private void RepositoryOnChanged(object sender, EventArgs eventArgs)
        {
            UpdateRepositoryValues();
        }

        private void UpdateRepositoryValues()
        {
            ImageItemCount = _repository.Catalog.Count(x => x.MediaType == MediaType.Image);
            VideoItemCount = _repository.Catalog.Count(x => x.MediaType == MediaType.Video);
            ItemCount = ImageItemCount + VideoItemCount;
            Size = (int) (IOHelper.DirectorySize(_repository.Location.LocalPath) / (long)Math.Pow(2,20));
        }

        public int ItemCount
        {
            get { return _imageItemCount; }
            set { SetValue(ref _imageItemCount, value); }
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

        public int Size
        {
            get { return _size; }
            set { SetValue(ref _size, value); }
        }

        public string Location => _repository.Location.LocalPath;
    }
}
