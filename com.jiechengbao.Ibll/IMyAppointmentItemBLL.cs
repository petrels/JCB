﻿using com.jiechengbao.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiechengbao.Ibll
{
    public interface IMyAppointmentItemBLL
    {
        IEnumerable<MyAppointmentItem> GetByMyAppointmentId(Guid myappointmentId);
        bool Add(MyAppointmentItem appointmentItem);
    }
}
