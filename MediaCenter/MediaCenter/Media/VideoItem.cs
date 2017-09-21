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

        public VideoItem(string name) : base(name)
        {

        }

        public byte[] FirstFrameImage
        {
            get => _firstFrameImage;
            set => SetValue(ref _firstFrameImage, value);
        }

        public string VideoFilePath { get; set; }

        public override void UpdateFrom(MediaItem item)
        {
            base.UpdateFrom(item);

            if (item is VideoItem)
                VideoFilePath = ((VideoItem) item).VideoFilePath;
        }
    }
}
