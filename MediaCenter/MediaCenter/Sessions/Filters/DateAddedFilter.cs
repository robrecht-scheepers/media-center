using System;
using System.Collections.Generic;
using System.Linq;
using MediaCenter.Media;

namespace MediaCenter.Sessions.Filters
{
    public class DateAddedFilter : DatePeriodFilter
    {
        public static string Name = "Date added";

        public override IEnumerable<MediaItem> Apply(IEnumerable<MediaItem> source)
        {
            if (!From.HasValue && !Until.HasValue)
                return source;

            return Invert
                ? source.Where(x => x.DateAdded.Date < From?.Date || x.DateAdded.Date > Until?.Date)
                : source.Where(x => x.DateAdded.Date >= From?.Date && x.DateAdded.Date <= Until?.Date);
        }
    }
}
