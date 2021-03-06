﻿using com.jiechengbao.entity;
using com.jiechengbao.Idal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiechengbao.dal
{
    public class AddressDAL : DataBaseDAL<Address>, IAddressDAL
    {
        public IEnumerable<Address> SelectByMemberId(Guid memberId)
        {
            try
            {
                return db.Set<Address>().Where(n => n.MemberId == memberId);
            }
            catch (Exception ex)
            {
                LogHelper.Log.Write(ex.Message);
                LogHelper.Log.Write(ex.StackTrace);
                throw;
            }
        }
    }
}
