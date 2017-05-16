using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.Repository;

namespace MediaCenter.Sessions
{
    public class QuerySession : SessionBase
    {
        public QuerySession(MediaRepository repository) : base(repository)
        {
        }
    }
}
