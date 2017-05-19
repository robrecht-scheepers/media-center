using System.Runtime.Serialization;

namespace MediaCenter.MediaItems
{
    [DataContract(Name = "Image")]
    public class ImageItem : MediaItem
    {
        public ImageItem(string name) : base(name)
        {
        }
    }
}
