using System;

namespace MediaCenter.Sessions.Query.Filters
{
    public abstract class DatePeriodFilter : Filter
    {
        private DateTime? _from;
        public DateTime? From
        {
            get { return _from; }
            set
            {
                if (Until?.Date == From?.Date)
                    Until = null;

                SetValue(ref _from, value, FromUpdated);
            }
        }
        private void FromUpdated()
        {
            if (From == null)
                return;

            if (Until == null)
                Until = From;
        }
        private DateTime? _until;
        public DateTime? Until
        {
            get { return _until; }
            set { SetValue(ref _until, value); }
        }
    }
}
