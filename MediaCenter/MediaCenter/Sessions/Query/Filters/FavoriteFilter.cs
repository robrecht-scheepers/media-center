using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.Repository;

namespace MediaCenter.Sessions.Query.Filters
{
    public class FavoriteFilter : Filter
    {
        private bool _favorite;

        public static string Name = "Favorite";

        public bool Favorite
        {
            get { return _favorite; }
            set { SetValue(ref _favorite, value); }
        }

        public override IEnumerable<MediaInfo> Apply(IEnumerable<MediaInfo> source)
        {
            switch (FilterMode)
            {
                case FilterMode.Match:
                    return source.Where(x => x.Favorite == Favorite);
                case FilterMode.NoMatch:
                    return source.Where(x => x.Favorite != Favorite);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
