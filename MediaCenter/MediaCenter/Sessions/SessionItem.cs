using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.MVVM;
using MediaCenter.Repository;

namespace MediaCenter.Sessions
{
    public abstract class SessionItem : Observable
    {
        public string Name => Info.Name;
        public MediaInfo Info { get; set; }

        private byte[] _thumbnail;
        public byte[] Thumbnail
        {
            get { return _thumbnail; }
            set { SetValue(ref _thumbnail, value); }
        }

    }
}
