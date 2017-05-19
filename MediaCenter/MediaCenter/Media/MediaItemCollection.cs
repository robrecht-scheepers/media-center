﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MediaCenter.Media
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