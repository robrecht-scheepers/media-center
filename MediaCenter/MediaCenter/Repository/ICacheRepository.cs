using System.Threading.Tasks;
using MediaCenter.Media;

namespace MediaCenter.Repository
{
    public interface ICacheRepository : IBaseRepository
    {
        Task AddToCache(MediaItem item, string filePath, string thumbnailPath);
        Task RemoveFromCache(MediaItem item);
    }
}
