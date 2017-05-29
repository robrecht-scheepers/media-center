using System;
using System.Collections.Generic;
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
        public bool Favorite { get; set; }

        public void UpdateFrom(MediaInfo item)
        {
            Name = item.Name;
            DateTaken = item.DateTaken;
            Favorite = item.Favorite;
            Tags.Clear();
            Tags.InsertRange(0,item.Tags);
        }
    }
}
