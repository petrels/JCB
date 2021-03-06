﻿using com.jiechengbao.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiechengbao.Ibll
{
    public interface IServiceBLL
    {
        bool Add(MyService ms);
        MyService GetMyServiceByServiceId(Guid serviceId);
        bool Update(MyService ms);
        IEnumerable<MyService> GetMyServiceByMemberId(Guid memberId);
    }
}
