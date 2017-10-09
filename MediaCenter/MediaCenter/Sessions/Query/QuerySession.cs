using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
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
        private readonly int PrefetchBufferSize = int.Parse(ConfigurationManager.AppSettings["PrefetchBufferSize"]);

        public QuerySession(IRepository repository) : base(repository)
        {
            Filters = new ObservableCollection<Filter>();
            QueryResult = new ObservableCollection<MediaItem>();
        }

        public ObservableCollection<Filter> Filters { get; } 

        public ObservableCollection<MediaItem> QueryResult { get; }

        public async Task ExecuteQuery()
        {
            var items = Filters.Aggregate(Repository.Catalog, (current, filter) => filter.Apply(current)).ToList();

            // if there is no private filter set by the user, filter all private items by default
            if(!Filters.Any(x => x is PrivateFilter))
            {
                var privateFilter = new PrivateFilter { PrivateSetting = PrivateFilter.PrivateOption.NoPrivate };
                items = privateFilter.Apply(items).ToList();
            }

            items.Sort((x,y) => DateTime.Compare(x.DateTaken,y.DateTaken));

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

        public async Task LoadFullImage(string name)
        {
            // decide other prefetch items
            var prefetchList = new List<string>();
            var index = QueryResult.IndexOf(QueryResult.First(x => x.Name == name));
            if (index < QueryResult.Count - 1)
            {
                for (int i = 1; i < PrefetchBufferSize && index + i < QueryResult.Count; i++)
                {
                    if(QueryResult[index + i].MediaType == MediaType.Image)
                        prefetchList.Add(QueryResult[index + i].Name);
                }
            }
            if (index > 0 && QueryResult[index - 1].MediaType == MediaType.Image)
            {
                prefetchList.Add(QueryResult[index - 1].Name);
            }
            var item = QueryResult.First(x => x.Name == name);
            if(item.MediaType == MediaType.Image)
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

        public async Task DeleteItem(MediaItem item)
        {
            if (item == null)
                return;

            if (QueryResult.Contains(item))
                QueryResult.Remove(item);
            await Repository.DeleteItem(item.Name);
        }
    }
}
