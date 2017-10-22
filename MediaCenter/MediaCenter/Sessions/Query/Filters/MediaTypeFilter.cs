using System.Collections.Generic;
using System.Linq;
using MediaCenter.Media;

namespace MediaCenter.Sessions.Query.Filters
{
    public class MediaTypeFilter : Filter
    {
        public static string Name = "Media type";

        public MediaTypeFilter()
        {
            MediaType = MediaType.Image;
        }

        public override IEnumerable<MediaItem> Apply(IEnumerable<MediaItem> source)
        {
            return source.Where(x => x.MediaType == MediaType);
        }

        private MediaType _mediaType;
        public MediaType MediaType { get { return _mediaType; } set { SetValue(ref _mediaType, value); } }
        public List<MediaType> MediaTypes => new List<MediaType>() { MediaType.Image, MediaType.Video };
    }
}
