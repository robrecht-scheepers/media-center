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
    public class SessionItem : Observable
    {
        private Image _thumbnail;
        private Image _fullImage;

        public MediaInfo Info { get; set; }

        public Image Thumbnail
        {
            get { return _thumbnail; }
            set { SetValue(ref _thumbnail, value); }
        }

        public Image FullImage
        {
            get { return _fullImage; }
            set { SetValue(ref _fullImage, value); }
        }
    }
}
