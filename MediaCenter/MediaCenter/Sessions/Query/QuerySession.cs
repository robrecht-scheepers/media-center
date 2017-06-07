using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.RightsManagement;
using System.Threading.Tasks;
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
            QueryResult = new ObservableCollection<QueryResultItem>();
        }

        public ObservableCollection<Filter> Filters { get; } 

        public ObservableCollection<QueryResultItem> QueryResult { get; }

        public async Task ExecuteQuery()
        {
            var infos = Filters.Aggregate(Repository.Catalog, (current, filter) => filter.Apply(current));

            QueryResult.Clear();
            foreach (var mediaInfo in infos)
            {
                QueryResult.Add(new QueryResultItem{ Info = mediaInfo });
            }

            foreach (var item in QueryResult)
            {
                item.Thumbnail = await Repository.GetThumbnailBytes(item.Name);
            }
        }

        public async Task FullImageRequested(string name)
        {
            if (name == CurrentImageName)
                return;

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

            CurrentFullImage = await Repository.GetFullImage(name, prefetchList);
        }

        private string _currentImageName;

        public string CurrentImageName
        {
            get { return _currentImageName; }
            set { SetValue(ref _currentImageName, value); }
        }

        private byte[] _currentFullImage;
        public byte[] CurrentFullImage  
        {
            get { return _currentFullImage; }
            set { SetValue(ref _currentFullImage,value); }
        }

        public async Task SaveItem(MediaInfo info)
        {
            await Repository.SaveItemInfo(info);
        }
    }
}
