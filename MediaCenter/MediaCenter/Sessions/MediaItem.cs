using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using MediaCenter.MVVM;

namespace MediaCenter.Repository
{
    [DataContract]
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

        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public DateTime DateTaken { get; set; }

        [DataMember]
        public DateTime DateAdded { get; set; }

        [DataMember]
        public MediaType Type { get; set; }

        [DataMember]
        public ObservableCollection<string> Tags { get; set; }

        private bool _favorite;
        [DataMember]
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

        //public override bool Equals(object obj)
        //{
        //    var other = obj as MediaInfo;
        //    if (other == null)
        //        return false;

        //    if (Name != other.Name || DateTaken != other.DateTaken || Favorite != other.Favorite)
        //        return false;

        //    if (other.Tags.Any(t => !this.Tags.Contains(t)))
        //        return false;

        //    if (Tags.Any(t => !other.Tags.Contains(t)))
        //        return false;

        //    return true;
        //}

        //public MediaInfo Clone()
        //{
        //    var info = new MediaInfo(Name);
        //    info.UpdateFrom(this);
        //    return info;
        //}
    }
}
