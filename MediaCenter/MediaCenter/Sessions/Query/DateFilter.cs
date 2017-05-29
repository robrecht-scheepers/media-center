using System;
using System.Collections.Generic;
using System.Linq;
using MediaCenter.Repository;

namespace MediaCenter.Sessions.Query
{
    public class DateFilter : Filter
    {
        private DateTime _from;
        public DateTime From
        {
            get { return _from; }
            set { SetValue(ref _from,value); }
        }

        private DateTime _until;
        public DateTime Until
        {
            get { return _until; }
            set { SetValue(ref _until, value); }
        }

        public override IEnumerable<MediaInfo> Apply(IEnumerable<MediaInfo> source, FilterMode filterMode)
        {
            switch (filterMode)
            {
                case FilterMode.Match:
                    return source.Where(x => x.DateTaken >= From && x.DateTaken <= Until);
                case FilterMode.NoMatch:
                    return source.Where(x => x.DateTaken < From || x.DateTaken > Until);
                default:
                    throw new ArgumentOutOfRangeException(nameof(filterMode), filterMode, null);
            }
            
        }
    }
}
