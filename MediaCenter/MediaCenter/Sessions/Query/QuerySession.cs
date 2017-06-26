using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MediaCenter.Media;
using MediaCenter.Repository;
using MediaCenter.Sessions.Query.Filters;

namespace MediaCenter.Sessions.Query
{
    public class QuerySession : SessionBase
    {
        private readonly int _numberOfPrefetchItems = 3;

        public QuerySession(RemoteRepository repository) : base(repository)
        {
            Filters = new ObservableCollection<Filter>();
            QueryResult = new ObservableCollection<MediaItem>();
        }

        public ObservableCollection<Filter> Filters { get; } 

        public ObservableCollection<MediaItem> QueryResult { get; }

        public async Task ExecuteQuery()
        {
            var items = Filters.Aggregate(Repository.Catalog, (current, filter) => filter.Apply(current));

            QueryResult.Clear();
            foreach (var item in items)
            {
                QueryResult.Add(item);
            }

            foreach (var item in QueryResult)
            {
                item.Thumbnail = await Repository.GetThumbnail(item.Name);
            }
        }

        public async Task ContentRequested(string name)
        {
            // decide other prefetch items
            var prefetchList = new List<string>();
            var index = QueryResult.IndexOf(QueryResult.First(x => x.Name == name));
            if (index < QueryResult.Count - 1)
            {
                for (int i = 1; i < _numberOfPrefetchItems && index + i < QueryResult.Count; i++)
                {
                    prefetchList.Add(QueryResult[index + i].Name);
                }
            }
            if (index > 0)
                prefetchList.Add(QueryResult[index - 1].Name);
            var item = QueryResult.First(x => x.Name == name);

            item.Content = await Repository.GetFullImage(name, prefetchList);
        }
        

        public async Task SaveItem(MediaItem item)
        {
            if (item == null)
                return;

            if (item.IsInfoDirty)
            {
                await Repository.SaveItemInfo(item.Name);
                item.IsInfoDirty = false;
            }

            if (item.IsContentDirty)
            {
                await Repository.SaveItemContent(item.Name);
                item.IsContentDirty = false;
            }

            if(item.IsThumbnailDirty)
            {
                await Repository.SaveItemThumbnail(item.Name);
                item.IsThumbnailDirty = false;
            }
        }
    }
}
