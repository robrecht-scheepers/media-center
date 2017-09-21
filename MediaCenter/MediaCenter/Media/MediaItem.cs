using System;
using System.Collections.ObjectModel;
using MediaCenter.MVVM;

namespace MediaCenter.Media
{
    public abstract class MediaItem : PropertyChangedNotifier
    {
        protected MediaItem(string name)
        {
            Name = name;
            Tags = new ObservableCollection<string>();
        }

        public bool IsInfoDirty { get; set; }
        public bool IsContentDirty { get; set; }
        public bool IsThumbnailDirty { get; set; }

        public string Name { get; set; }

        private MediaItemStatus _status;
        public MediaItemStatus Status
        {
            get { return _status; }
            set { SetValue(ref _status,  value); }
        }

        public DateTime DateTaken { get; set; }

        public DateTime DateAdded { get; set; }
        
        public ObservableCollection<string> Tags { get; set; }

        private bool _favorite;
        public bool Favorite
        {
            get { return _favorite; }
            set { SetValue(ref _favorite, value, () => IsInfoDirty = true); }
        }

        private bool _private;
        public bool Private
        {
            get { return _private; }
            set { SetValue(ref _private, value, () => IsInfoDirty = true); }
        }

        private byte[] _thumbnail;

        public byte[] Thumbnail
        {
            get { return _thumbnail; }
            set { SetValue(ref _thumbnail, value); }
        }

        private byte[] _content;
        
        public byte[] Content
        {
            get { return _content; }
            set {SetValue(ref _content, value); }
        }

        public virtual void UpdateFrom(MediaItem item)
        {
            Name = item.Name;
            DateTaken = item.DateTaken;
            DateAdded = item.DateAdded;
            Favorite = item.Favorite;
            Tags.Clear();
            foreach (var tag in item.Tags)
            {
                Tags.Add(tag);
            }
            Thumbnail = item.Thumbnail;
        }
    }
}
