using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.MVVM;

namespace MediaCenter.Sessions
{
    public abstract class SessionViewModelBase : PropertyChangedNotifier
    {
        protected readonly SessionBase Session;

        protected SessionViewModelBase(SessionBase session)
        {
            Session = session;
        }

        public abstract string Name { get; }
    }
}
