﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.Repository;

namespace MediaCenter.Sessions
{
    public abstract class SessionBase
    {
        protected MediaRepository Repository;
        protected SessionBase(MediaRepository repository)
        {
            Repository = repository;
        }
    }
}
