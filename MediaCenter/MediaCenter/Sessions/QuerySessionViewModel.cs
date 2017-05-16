using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.MVVM;

namespace MediaCenter.Sessions
{
    public class QuerySessionViewModel : SessionViewModelBase
    {
        public QuerySessionViewModel(SessionBase session) : base(session)
        {
        }

        protected override string CreateNameForSession(SessionBase session)
        {
            return "Query 1";
        }
    }
}
