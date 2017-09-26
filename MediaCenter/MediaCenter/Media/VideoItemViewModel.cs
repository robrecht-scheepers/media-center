using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.MVVM;

namespace MediaCenter.Media
{
    public class VideoItemViewModel : MediaItemViewModel
    {
        public VideoItemViewModel(MediaItem item)
        {
            if (item.MediaType != MediaType.Video)
                throw new ArgumentException($"Cannot create video view model for non-video item. Item type is {item.MediaType}.");
            MediaItem = item;
        }
        public MediaItem MediaItem { get; }
    }

    
}
