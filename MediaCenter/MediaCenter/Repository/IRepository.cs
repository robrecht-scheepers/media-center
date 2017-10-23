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
        IEnumerable<MediaItem> Catalog { get; }
        IEnumerable<string> Tags { get; }
        Task Initialize();
        Task SaveNewItems(IEnumerable<StagedItem> newItems); 
        Task DeleteItem(string name);
        Task<byte[]> GetThumbnail(string name);
        Task<byte[]> GetFullImage(string name, IEnumerable<string> prefetch);
        Task SaveItemInfo(string name);
        Task SaveItemContent(string name);
        Task SaveItemThumbnail(string name);
        event EventHandler Changed;
        System.Uri Location { get; }
        Task SaveContentToFile(string itemName, string filePath);
        string ItemNameToContentFileName(string itemName);
    }
}
