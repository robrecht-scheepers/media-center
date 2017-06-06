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

            // TODO: implement prefetching and error handling

            // decide other prefetch items
            int index = QueryResult.IndexOf(QueryResult.First(x => x.Name == name));
            var prefetch = new List<string>();
            for (int i = 1; i < 3 && i < QueryResult.Count; i++)
            {
                if(index+i < QueryResult.Count)
                    prefetch.Add(QueryResult[index + i].Name);
            }

            CurrentFullImage = await Repository.GetFullImage(name, prefetch);
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
            //// TODO: why is this necessary? Items should be updated by repository, apprently is a copy and not a reference?
            //var item = QueryResult.First(i => i.Name == info.Name);
            //item.Info.UpdateFrom(info);
            await Repository.SaveItemInfo(info);
        }
    }
}
