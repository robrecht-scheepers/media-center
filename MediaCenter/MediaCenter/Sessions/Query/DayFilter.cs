using System;
using System.Collections.Generic;
using System.Linq;
using MediaCenter.Repository;

namespace MediaCenter.Sessions.Query
{
    public class DayFilter : Filter
    {
        public static string Name = "Day taken";

        private DateTime _day;
        public DateTime Day
        {
            get { return _day; }
            set { SetValue(ref _day, value); }
        }

        
        public override IEnumerable<MediaInfo> Apply(IEnumerable<MediaInfo> source)
        {
            switch (FilterMode)
            {
                case FilterMode.Match:
                    return source.Where(x => x.DateTaken.Date == Day.Date);
                case FilterMode.NoMatch:
                    return source.Where(x => x.DateTaken.Date != Day.Date);
                default:
                    throw new ArgumentOutOfRangeException(nameof(FilterMode), FilterMode, null);
            }
            
        }
    }
}
