﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.MediaItems;

namespace MediaCenter.Sessions.Query
{
    public class DateCriterion : Criterion
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


        public override IEnumerable<MediaItem> Filter(IEnumerable<MediaItem> source, FilterMode filterMode)
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
