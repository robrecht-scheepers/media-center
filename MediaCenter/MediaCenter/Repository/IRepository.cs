using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaCenter.Media;
using MediaCenter.Sessions.Staging;

namespace MediaCenter.Repository
{
    public interface IRepository
    {
        event EventHandler CollectionChanged;
        event EventHandler StatusChanged;

        IEnumerable<MediaItem> Catalog { get; }
        IEnumerable<string> Tags { get; }
        Task Initialize();
        Task SaveNewItems(IEnumerable<StagedItem> newItems); 
        Task DeleteItem(MediaItem item);
        Task<byte[]> GetThumbnail(MediaItem item);
        Task<byte[]> GetFullImage(MediaItem item, IEnumerable<MediaItem> prefetch);
        Task SaveItem(MediaItem item);
        System.Uri Location { get; }
        Task SaveContentToFile(MediaItem item, string filePath);
        Task SaveMultipleContentToFolder(List<MediaItem> items, string folderPath);
        string StatusMessage { get; }
    }
}
