using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task SaveItem(string name)
        {
            await Repository.SaveItem(name);
        }
    }
}
