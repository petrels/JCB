﻿using com.jiechengbao.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiechengbao.Ibll
{
    public interface IServiceQRBLL
    {
        bool Add(ServiceQR qr);
        ServiceQR GetServiceQRByServcieId(Guid serviceId);
    }
}