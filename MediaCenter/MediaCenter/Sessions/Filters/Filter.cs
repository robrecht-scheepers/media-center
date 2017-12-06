using System.Collections.Generic;
using MediaCenter.Media;
using MediaCenter.MVVM;

namespace MediaCenter.Sessions.Filters
{
    public abstract class Filter : PropertyChangedNotifier
    {
        private bool _invert;

        protected Filter()
        {
            Invert = false;
        }

        public bool Invert
        {
            get { return _invert; }
            set { SetValue(ref _invert, value); }
        }

        public abstract IEnumerable<MediaItem> Apply(IEnumerable<MediaItem> source);

        public static Filter Create(string name, List<string> tags = null)
        {
            if (name == DateTakenFilter.Name)
                return new DateTakenFilter();
            else if (name == TagFilter.Name)
                return new TagFilter(tags);
            else if (name == FavoriteFilter.Name)
                return new FavoriteFilter();
            else if (name == DateAddedFilter.Name)
                return new DateAddedFilter();
            else if (name == PrivateFilter.Name)
                return new PrivateFilter();
            else if (name == MediaTypeFilter.Name)
                return new MediaTypeFilter();
            else return null;
        }
        public static string GetName(Filter filter)
        {
            if (filter is DateTakenFilter)
                return DateTakenFilter.Name;
            else if (filter is TagFilter)
                return TagFilter.Name;
            else if (filter is FavoriteFilter)
                return FavoriteFilter.Name;
            else if (filter is DateAddedFilter)
                return DateAddedFilter.Name;
            else if (filter is PrivateFilter)
                return PrivateFilter.Name;
            else if (filter is MediaTypeFilter)
                return MediaTypeFilter.Name;
            else return "";
        }
    }
}
