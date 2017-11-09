using System;
using System.Collections.Generic;
using System.Linq;
using MediaCenter.Media;

namespace MediaCenter.Sessions.Filters
{
    public class DateTakenFilter : DatePeriodFilter
    {
        public static string Name = "Date taken";

        public override IEnumerable<MediaItem> Apply(IEnumerable<MediaItem> source)
        {
            if (!From.HasValue && !Until.HasValue)
                return source;

            return Invert
                ? source.Where(x => x.DateTaken.Date < From?.Date || x.DateTaken.Date > Until?.Date)
                : source.Where(x => x.DateTaken.Date >= From?.Date && x.DateTaken.Date <= Until?.Date);
        }
    }
}
