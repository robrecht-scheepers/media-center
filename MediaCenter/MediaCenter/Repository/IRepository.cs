using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        Task DeleteItem(string name);
        Task<byte[]> GetThumbnail(string name);
        Task<byte[]> GetFullImage(string name, IEnumerable<string> prefetch);
        Task SaveItem(string name);
        System.Uri Location { get; }
        Task SaveContentToFile(MediaItem item, string filePath);
        Task SaveContentToFolder(List<MediaItem> items, string folderPath);
        string StatusMessage { get; }
    }
}
