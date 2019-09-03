using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaCenter.Media;
using MediaCenter.Sessions.Filters;
using MediaCenter.Sessions.Staging;

namespace MediaCenter.Repository
{
    public interface IRepository : IBaseRepository
    {
        Task SaveNewItems(IEnumerable<StagedItem> newItems); 
        Task DeleteItem(MediaItem item);
        Task SaveItem(MediaItem item);
    }
}
