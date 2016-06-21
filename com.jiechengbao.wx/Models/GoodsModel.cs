﻿using com.jiechengbao.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace com.jiechengbao.wx.Models
{
    public class GoodsModel:Goods
    {
        public GoodsModel() { }
        public GoodsModel(Goods goods)
        {
            this.Code = goods.Code;
            this.CreatedTime = goods.CreatedTime;
            this.DeletedTime = goods.DeletedTime;
            this.Discount = goods.Discount;
            this.Id = goods.Id;
            this.IsDeleted = goods.IsDeleted;
            this.Name = goods.Name;
            this.Price = goods.Price;
            this.Description = goods.Description;
        }

        [DataMember]
        public string PicturePath { get; set; }
        public string[] CategoryList { get; set; }
    }
}