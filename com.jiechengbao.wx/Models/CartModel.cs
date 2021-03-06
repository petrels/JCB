﻿using com.jiechengbao.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.jiechengbao.wx.Models
{
    public class CartModel:Goods
    {
        public CartModel() { }
        public CartModel(Goods goods) {
            this.Code = goods.Code;
            this.CreatedTime = goods.CreatedTime;
            this.DeletedTime = goods.DeletedTime;
            this.Id = goods.Id;
            this.IsDeleted = goods.IsDeleted;
            this.Name = goods.Name;
            this.Price = goods.Price;
            this.Description = goods.Description;
            this.ServiceCount = goods.ServiceCount;
            this.OriginalPrice = goods.OriginalPrice;
            this.ExchangeCredit = goods.ExchangeCredit;
        }
        public double Discount { get; set; }
        public int Count { get; set; }
        public string PicturePath { get; set; }
    }
}