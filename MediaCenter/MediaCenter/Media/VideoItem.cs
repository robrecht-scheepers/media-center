using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCenter.Media
{
    public class VideoItem : MediaItem
    {
        private byte[] _firstFrameImage;

        public VideoItem(string name) : base(name, MediaType.Video)
        {

        }

        public byte[] FirstFrameImage
        {
            get => _firstFrameImage;
            set => SetValue(ref _firstFrameImage, value);
        }
    }
}
