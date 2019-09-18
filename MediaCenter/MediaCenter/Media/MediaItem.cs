using System;
using System.Collections.ObjectModel;
using System.Drawing;
using MediaCenter.MVVM;

namespace MediaCenter.Media
{
    public class MediaItem : PropertyChangedNotifier
    {
        public MediaItem(MediaType type) : this("", type)
        {
            
        }

        public MediaItem(string name, MediaType type)
        {
            Name = name;
            Tags = new ObservableCollection<string>();
            MediaType = type;
        }

        public bool IsInfoDirty { get; set; }
        public bool IsContentDirty { get; set; }
        public bool IsThumbnailDirty { get; set; }

        public bool IsDirty => IsInfoDirty || IsContentDirty || IsThumbnailDirty;

        public int Id { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetValue(ref _name, value);
        }

        public MediaType MediaType { get; set; }

        private MediaItemStatus _status;
        public MediaItemStatus Status
        {
            get => _status;
            set => SetValue(ref _status,  value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetValue(ref _statusMessage, value);
        }

        private DateTime _dateTaken;
        public DateTime DateTaken
        {
            get => _dateTaken;
            set => SetValue(ref _dateTaken, value);
        }

        public DateTime DateAdded { get; set; }
        
        public ObservableCollection<string> Tags { get; set; }

        private bool _favorite;
        public bool Favorite
        {
            get => _favorite;
            set { SetValue(ref _favorite, value, () => IsInfoDirty = true); }
        }

        private bool _private;
        public bool Private
        {
            get => _private;
            set { SetValue(ref _private, value, () => IsInfoDirty = true); }
        }

        private byte[] _thumbnail;

        public byte[] Thumbnail
        {
            get => _thumbnail;
            set => SetValue(ref _thumbnail, value);
        }

        private byte[] _content;
        
        public byte[] Content
        {
            get => _content;
            set => SetValue(ref _content, value);
        }

        private Uri _contentUri;
        public Uri ContentUri
        {
            get => _contentUri;
            set => _contentUri = value;
        }

        private string _contentFileName;
        public string ContentFileName
        {
            get => _contentFileName;
            set => SetValue(ref _contentFileName, value);
        }

        private int _rotation;
        private string _statusMessage;

        public int Rotation
        {
            get => _rotation;
            set => SetValue(ref _rotation, value);
        }

        private Crop _crop;
        public Crop Crop
        {
            get => _crop;
            set => SetValue(ref _crop, value);
        }

        public virtual void UpdateFrom(MediaItem item)
        {
            Name = item.Name;
            MediaType = item.MediaType;
            DateTaken = item.DateTaken;
            DateAdded = item.DateAdded;
            Favorite = item.Favorite;
            Private = item.Private;
            Crop = item.Crop;
            Tags.Clear();
            foreach (var tag in item.Tags)
            {
                Tags.Add(tag);
            }
            Thumbnail = item.Thumbnail;
            ContentUri = item.ContentUri;
        }
    }
}
