using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.Helpers;

namespace MediaCenter.Media
{
    public class ImageContent : MediaContent
    {
        public ImageContent(string name, byte[] image) : base(name)
        {
            Image = image;
        }

        private byte[] _image;
        public byte[] Image
        {
            get { return _image; }
            set { SetValue(ref _image, value); }
        }

        public void RotateImage(RotationDirection direction)
        {
            Image = ImageHelper.Rotate(Image, direction);
            IsDirty = true;
        }

        public override byte[] Content => Image;
    }
}
