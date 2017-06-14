﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.MVVM;

namespace MediaCenter.Media
{
    public abstract class MediaContent : PropertyChangedNotifier
    {
        protected MediaContent(string name)
        {
            Name = name;
        }

        public bool IsDirty { get; set; }

        public string Name { get; private set; }
    }
}
