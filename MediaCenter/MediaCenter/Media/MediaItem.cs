using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace MediaCenter.Media
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
    }
}
