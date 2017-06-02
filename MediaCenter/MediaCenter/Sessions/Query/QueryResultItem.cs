using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCenter.Sessions.Query
{
    public class QueryResultItem : SessionItem
    {
        private byte[] _thumbnail;

        public byte[] Thumbnail
        {
            get { return _thumbnail; }
            set { SetValue(ref _thumbnail, value); }
        }
    }
}
