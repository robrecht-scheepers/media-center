using System;
using System.Collections.Generic;
using System.Linq;
using MediaCenter.Repository;

namespace MediaCenter.Sessions.Query.Filters
{
    public class DateAddedFilter : DatePeriodFilter
    {
        public static string Name = "Date added";

        public override IEnumerable<MediaInfo> Apply(IEnumerable<MediaInfo> source)
        {
            switch (FilterMode)
            {
                case FilterMode.Match:
                    return source.Where(x => x.DateAdded.Date >= From?.Date && x.DateAdded.Date <= Until?.Date);
                case FilterMode.NoMatch:
                    return source.Where(x => x.DateAdded.Date < From?.Date || x.DateAdded.Date > Until?.Date);
                default:
                    throw new ArgumentOutOfRangeException(nameof(FilterMode), FilterMode, null);
            }

        }
    }
}
