using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MediaCenter.Media
{
    [DataContract(Name = "Image")]
    public class ImageItem : MediaItem
    {
        public ImageItem(string name) : base(name)
        {
        }
    }
}
