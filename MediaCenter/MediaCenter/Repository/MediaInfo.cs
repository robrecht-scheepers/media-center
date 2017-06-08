using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MediaCenter.Repository
{
    [DataContract]
    public class MediaInfo
    {
        public MediaInfo(string name)
        {
            Name = name;
            Tags = new List<string>();
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<string> Tags { get; set; }

        [DataMember]
        public DateTime DateTaken { get; set; }

        [DataMember]
        public DateTime DateAdded { get; set; }

        [DataMember()]
        public MediaType Type { get; set; }

        [DataMember]
        public bool Favorite { get; set; }

        public void UpdateFrom(MediaInfo item)
        {
            Name = item.Name;
            DateTaken = item.DateTaken;
            DateAdded = item.DateAdded;
            Favorite = item.Favorite;
            Tags.Clear();
            Tags.InsertRange(0,item.Tags);
        }

        public override bool Equals(object obj)
        {
            var other = obj as MediaInfo;
            if (other == null)
                return false;

            if (Name != other.Name ||  DateTaken != other.DateTaken || Favorite != other.Favorite)
                return false;

            if (other.Tags.Any(t => !this.Tags.Contains(t)))
                return false;
            
            if (Tags.Any(t => !other.Tags.Contains(t)))
                return false;

            return true;
        }

        public MediaInfo Clone()
        {
            var info = new MediaInfo(Name);
            info.UpdateFrom(this);
            return info;
        }
    }
}
