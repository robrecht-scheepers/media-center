using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.Repository;

namespace MediaCenter.Sessions.Query.Filters
{
    public class TagFilter : Filter
    {
        public static string Name = "Tag";

        private string _tag;

        public string Tag
        {
            get { return _tag; }
            set { SetValue(ref _tag, value); }
        }

        public override IEnumerable<MediaInfo> Apply(IEnumerable<MediaInfo> source)
        {
            if (string.IsNullOrEmpty(Tag))
                return source;

            switch (FilterMode)
            {
                case FilterMode.Match:
                    return source.Where(x => x.Tags.Contains(Tag));
                case FilterMode.NoMatch:
                    return source.Where(x => !x.Tags.Contains(Tag));
                default:
                    throw new ArgumentOutOfRangeException(nameof(FilterMode), FilterMode, null);
            }
        }
    }
}
