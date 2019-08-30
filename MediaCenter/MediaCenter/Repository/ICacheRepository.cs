using MediaCenter.Media;

namespace MediaCenter.Repository
{
    public interface ICacheRepository : IBaseRepository
    {
        void AddToCache(MediaItem item);
        void RemoveFromCache(MediaItem item);
    }
}
