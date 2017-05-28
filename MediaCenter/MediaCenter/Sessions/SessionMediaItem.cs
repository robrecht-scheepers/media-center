using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.MVVM;

namespace MediaCenter.Sessions
{
    public class SessionMediaItem : Observable
    {
        public string Name { get; set; }
    }
}
