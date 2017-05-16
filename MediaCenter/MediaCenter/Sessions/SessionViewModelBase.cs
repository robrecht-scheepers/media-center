using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.MVVM;

namespace MediaCenter.Sessions
{
    public abstract class SessionViewModelBase : Observable
    {
        private SessionBase _session;

        protected SessionViewModelBase(SessionBase session)
        {
            _session = session;
        }


        public string Name => CreateNameForSession(_session);
        protected abstract string CreateNameForSession(SessionBase session);
    }
}
