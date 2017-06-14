using System;
using System.Collections.ObjectModel;
using MediaCenter.MVVM;

namespace MediaCenter.Media
{
    public class MediaItem : PropertyChangedNotifier
    {
        public MediaItem(string name, MediaType type)
        {
            Name = name;
            Type = type;
            Tags = new ObservableCollection<string>();
            Tags.CollectionChanged += (sender, args) => IsDirty = true;
        }

        public bool IsDirty { get; set; }

        public string Name { get; private set; }

        public DateTime DateTaken { get; set; }

        public DateTime DateAdded { get; set; }

        public MediaType Type { get; set; }

        public ObservableCollection<string> Tags { get; set; }

        private bool _favorite;
        public bool Favorite
        {
            get { return _favorite; }
            set { SetValue(ref _favorite, value, () => IsDirty = true); }
        }

        private byte[] _thumbnail;
        public byte[] Thumbnail
        {
            get { return _thumbnail; }
            set { SetValue(ref _thumbnail, value); }
        }

        public void UpdateFrom(MediaItem item)
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
