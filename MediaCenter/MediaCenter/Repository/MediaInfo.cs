using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using MediaCenter.Media;

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
            Type = item is VideoItem ? MediaType.Video : MediaType.Image;
            DateTaken = item.DateTaken;
            DateAdded = item.DateAdded;
            Favorite = item.Favorite;
            Private = item.Private;
            Tags = item.Tags.ToList();
        }

        public MediaItem ToMediaItem()
        {
            MediaItem item = null;
            switch (Type)
            {
                case MediaType.Video:
                    item = new VideoItem(Name);
                    break;
                default:
                    item = new ImageItem(Name);
                    break;
            }

            item.DateTaken = this.DateTaken;
            item.DateAdded = this.DateAdded;
            item.Favorite = this.Favorite;
            item.Private = this.Private;
            
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

        [DataMember]
        public bool Private { get; set; }


    }
}
