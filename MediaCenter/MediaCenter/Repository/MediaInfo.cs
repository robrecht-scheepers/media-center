using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using MediaCenter.Media;
using MediaCenter.Sessions;

namespace MediaCenter.Repository
{
    [DataContract]
    public class MediaInfo
    {
        public MediaInfo()
        {
            Tags = new List<string>();
        }

        public MediaInfo(MediaItem item)
        {
            Name = item.Name;
            Type = item.Type;
            DateTaken = item.DateTaken;
            DateAdded = item.DateAdded;
            Favorite = item.Favorite;
            Tags = item.Tags.ToList();
        }

        public MediaItem ToMediaItem()
        {
            var item = new MediaItem(Name, Type)
            {
                DateTaken = this.DateTaken,
                DateAdded = this.DateAdded,
                Favorite = this.Favorite
            };
            foreach (var tag in this.Tags)
            {
                item.Tags.Add(tag);
            }
            return item;
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

        
    }
}
