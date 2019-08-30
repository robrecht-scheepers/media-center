using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaCenter.Media;
using MediaCenter.Sessions.Filters;

namespace MediaCenter.Repository
{
    public interface IBaseRepository
    {
        event EventHandler CollectionChanged;
        event EventHandler StatusChanged;

        List<string> Tags { get; }
        Task Initialize();
        Task<List<MediaItem>> GetQueryItems(IEnumerable<Filter> filters);
        Task<int> GetQueryCount(IEnumerable<Filter> filters);
        Task<byte[]> GetThumbnail(MediaItem item);
        Task<byte[]> GetFullImage(MediaItem item, IEnumerable<MediaItem> prefetch = null);
        Uri GetContentUri(MediaItem item);
        System.Uri Location { get; }
        Task SaveContentToFile(MediaItem item, string filePath);
        Task SaveMultipleContentToFolder(List<MediaItem> items, string folderPath);
        string StatusMessage { get; }
    }
}
