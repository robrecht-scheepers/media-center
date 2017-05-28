using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.MVVM;
using MediaCenter.Repository;

namespace MediaCenter.Sessions
{
    public abstract class SessionBase : Observable
    {
        protected RemoteRepository Repository;
        protected SessionBase(RemoteRepository repository)
        {
            Repository = repository;
        }
    }
}
