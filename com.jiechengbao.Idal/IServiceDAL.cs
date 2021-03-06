﻿using com.jiechengbao.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiechengbao.Idal
{
    public interface IServiceDAL:IDataBaseDAL<MyService>
    {
        IEnumerable<MyService> SelectByMemberId(Guid memberId);
    }
}
