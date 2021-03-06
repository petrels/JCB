﻿using com.jiechengbao.entity;
using com.jiechengbao.Ibll;
using com.jiechengbao.wx.Global;
using com.jiechengbao.wx.Models;
using POPO.ActionFilter.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace com.jiechengbao.wx.Controllers
{
    [WhitespaceFilter]
    [ETag]
    /// <summary>
    /// 购物车 Controller
    /// </summary>
    public class CartController:Controller
    {
        private ICartBLL _cartBLL;
        private IMemberBLL _memberBLL;
        private IGoodsBLL _goodsBLL;
        private IGoodsImagesBLL _goodsImageBLL;
        private IRulesBLL _rulesBLL;
        public CartController(ICartBLL cartBLL, IMemberBLL memberBLL, IGoodsBLL goodsBLL,IGoodsImagesBLL goodsImagesBLL,IRulesBLL rulesBLL)
        {
            _cartBLL = cartBLL;
            _memberBLL = memberBLL;
            _goodsBLL = goodsBLL;
            _goodsImageBLL = goodsImagesBLL;
            _rulesBLL = rulesBLL;
        }

        /// <summary>
        /// 添加到购物车
        /// </summary>
        /// <param name="goodsCode"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Add(string goodsCode)
        {
            //先获得 goods 和 member 对象
            Goods goods = new Goods();
            Member member = new Member();
            bool res = false;
            try
            {
                goods = _goodsBLL.GetGoodsByCode(goodsCode);
                member = _memberBLL.GetMemberByOpenId(System.Web.HttpContext.Current.Session["member"] as string);
            }
            catch (Exception ex)
            {
                LogHelper.Log.Write(ex.Message);
                LogHelper.Log.Write(ex.StackTrace);
                throw;
            }

            // 判断是否已经存在购物车上
            if (!_cartBLL.IsInCart(member.Id,goods.Id))
            {
                // 不在购物车上时 直接添加到购物车上
                Cart cart = new Cart();
                cart.Id = Guid.NewGuid();
                cart.Count = 1;
                cart.CreatedTime = DateTime.Now.Date;
                cart.DeletedTime = DateTime.MinValue.AddHours(8);
                cart.GoodsId = goods.Id;
                cart.MemberId = member.Id;
                cart.IsDeleted = false;
                
                _cartBLL.Add(cart);
                res = true;
            }
            else
            {
                // 判断存在购物车上时  就取出 Cart 对象
                // 修改 cart 的数量
                Cart cart = _cartBLL.GetCartByMemberIdAndGoodsId(member.Id, goods.Id);
                cart.Count += 1;

                _cartBLL.Update(cart);
                res = true;
            }

            //统计购物车上的物品数量
            int count = 0;
            List<Cart> cartList = _cartBLL.GetCartByMemberId(member.Id).ToList();
            foreach (var item in cartList)
            {
                count += item.Count;
            }
            var obj = new { code = res, count = count };

            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        [IsLogin]
        public ActionResult List()
        {
            Member member = _memberBLL.GetMemberByOpenId(System.Web.HttpContext.Current.Session["member"] as string);

            // 商品的折扣和 会员的vip等级挂钩 
            // 所以这里要根据当前用户的vip等级找到对应的折扣
            double discount = _rulesBLL.GetDiscountByVIP(member.Vip);

            List<Cart> cartList = _cartBLL.GetCartByMemberId(member.Id).ToList();
            List<CartModel> cartModelList = new List<CartModel>();

            foreach (var item in cartList)
            {
                CartModel cm = new CartModel(_goodsBLL.GetGoodsById(item.GoodsId));

                GoodsImage gi = _goodsImageBLL.GetPictureByGoodsId(item.GoodsId);

                cm.PicturePath = gi.ImagePath;
                cm.Count = item.Count;
                cm.Discount = discount;

                cartModelList.Add(cm);
            }

            ViewData["CartModelList"] = cartModelList;
            return View();
        }

        /// <summary>
        /// 判断是否有商品在购物车上
        /// </summary>
        /// <returns></returns>
        public ActionResult Check()
        {
            bool res = true;
            Member member = _memberBLL.GetMemberByOpenId(System.Web.HttpContext.Current.Session["member"] as string);
            //统计购物车上的物品数量
            int count = 0;
            List<Cart> cartList = _cartBLL.GetCartByMemberId(member.Id).ToList();
            foreach (var item in cartList)
            {
                count += item.Count;
            }
            var obj = new { code = res, count = count };
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(string Code)
        {
            if (string.IsNullOrEmpty(Code))
            {
                return Json("False", JsonRequestBehavior.AllowGet);
            }

            Goods goods = _goodsBLL.GetGoodsByCode(Code);
            Member member = _memberBLL.GetMemberByOpenId(System.Web.HttpContext.Current.Session["member"] as string);
            Cart cart = _cartBLL.GetCartByMemberIdAndGoodsId(member.Id, goods.Id);

            if (_cartBLL.Remove(cart))
            {
                return Json("True", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("False", JsonRequestBehavior.AllowGet);
            }
        }
    }
}