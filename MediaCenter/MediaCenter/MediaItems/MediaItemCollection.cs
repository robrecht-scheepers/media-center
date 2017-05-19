using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MediaCenter.MediaItems
{
    [DataContract]
    [KnownType(typeof(ImageItem))]
    public class MediaItemCollection 
    {
        public MediaItemCollection()
        {
            Items = new List<MediaItem>();
        }

        [DataMember]
        public List<MediaItem> Items { get; set; }
    }
}
