using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using MediaCenter.Media;
using MediaCenter.Repository;
using MediaCenter.Sessions.Filters;

namespace MediaCenter.Sessions.Query
{
    public class QuerySession : SessionBase
    {
        private readonly int PrefetchBufferSize = int.Parse(ConfigurationManager.AppSettings["PrefetchBufferSize"]);

        public QuerySession(IRepository repository) : base(repository)
        {
            Filters = new List<Filter>();
            QueryResult = new ObservableCollection<MediaItem>();
        }

        public List<Filter> Filters { get; } 

        public ObservableCollection<MediaItem> QueryResult { get; }

        public async Task ExecuteQuery()
        {
            var tmpFiltersList = Filters.ToList();
            if (!tmpFiltersList.Any(x => x is PrivateFilter))
            {
                tmpFiltersList.Add(new PrivateFilter { PrivateSetting = PrivateFilter.PrivateOption.NoPrivate });
            }

            var items = tmpFiltersList.Aggregate(Repository.Catalog, (current, filter) => filter.Apply(current)).ToList();   
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

        public int CalculatMatchCount()
        {
            var tmpFiltersList = Filters.ToList();
            if (!tmpFiltersList.Any(x => x is PrivateFilter))
            {
                tmpFiltersList.Add(new PrivateFilter { PrivateSetting = PrivateFilter.PrivateOption.NoPrivate });
            }
            return tmpFiltersList.Aggregate(Repository.Catalog, (current, filter) => filter.Apply(current)).Count();
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
