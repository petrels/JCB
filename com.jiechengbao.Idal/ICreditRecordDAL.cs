﻿using com.jiechengbao.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiechengbao.Idal
{
    public interface ICreditRecordDAL:IDataBaseDAL<CreditRecord>
    {
        IEnumerable<CreditRecord> SelectByMemberId(Guid memberId);
    }
}
