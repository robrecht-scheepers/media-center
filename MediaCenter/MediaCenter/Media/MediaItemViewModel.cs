using MediaCenter.MVVM;

namespace MediaCenter.Media
{
    public abstract class MediaItemViewModel : PropertyChangedNotifier
    {
        public MediaItem MediaItem { get; protected set; }
    }
}
