using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace MediaCenter.Sessions
{
    public class StagingSessionViewModel : SessionViewModelBase
    {
        public StagingSessionViewModel(StagingSession session) : base(session)
        {
            
        }

        protected override string CreateNameForSession(SessionBase session)
        {
            return "Staging 1";
        }
    }
}
