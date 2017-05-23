using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MediaCenter.MediaItems
{

    [DataContract]
    public abstract class MediaItem
    {
        protected MediaItem(string name)
        {
            Name = name;
            Tags = new List<string>();
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<string> Tags { get; set; }

        public DateTime DateTaken { get; set; }

        public virtual void UpdateFrom(MediaItem item)
        {
            Name = item.Name;
            Tags.Clear();
            Tags.InsertRange(0,item.Tags);
        }
    }
}
