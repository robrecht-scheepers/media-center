﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCenter.Media
{
    public class VideoItem : MediaItem
    {
        public VideoItem(string name) : base(name, MediaType.Video)
        {

        }
    }
}
