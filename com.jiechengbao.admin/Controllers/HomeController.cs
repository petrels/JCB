﻿using com.jiechengbao.admin.Models;
using com.jiechengbao.entity;
using com.jiechengbao.Ibll;
using com.jiechengbao.Idal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace com.jiechengbao.admin.Controllers
{
    public class HomeController:Controller
    {
        private IMemberBLL _memberBLL;
        private IOrderBLL _orderBLL;
        private IOrderStatusBLL _orderStatusBLL;
        private IAddressBLL _addressBLL;

        public HomeController(IMemberBLL memberBLL,
            IOrderBLL orderBLL, IOrderStatusBLL orderStatusBLL,
            IAddressBLL addressBLL)
        {
            _memberBLL = memberBLL;
            _orderBLL = orderBLL;
            _orderStatusBLL = orderStatusBLL;
            _addressBLL = addressBLL;
        }        

        /// <summary>
        /// 管理员后台首页 
        /// 登陆成功后进入此页
        /// 显示 各种数据信息 包括 当前用户数量 当前新提交的订单数量 
        /// 这里用图表表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            ViewBag.MemberCount = _memberBLL.GetAllNoDeletedMembersCount();

            sw.Stop();
            LogHelper.Log.Write("Home/Index: GetAllNoDeletedMembersCount " + sw.ElapsedMilliseconds + "ms");

            sw.Restart();
            IEnumerable<Member> memberList = _memberBLL.GetNewMembersAtYesterDay();

            sw.Stop();
            LogHelper.Log.Write("Home/Index: GetNewMembersAtYesterDay " + sw.ElapsedMilliseconds + "ms");

            ViewData["MemberList"] = memberList;


            sw.Restart();

            // 获取昨天新提交的订单
            IEnumerable<Order> orderList = _orderBLL.GetYesterDayOrders();

            sw.Stop();
            LogHelper.Log.Write("Home/Index: GetYesterDayOrders " + sw.ElapsedMilliseconds + "ms");

            // 昨天新提交的订单的数量
            ViewBag.OrderCount = orderList.Count();

            List<OrderModel> orderModelList = new List<OrderModel>();

            sw.Restart();


            // 构造OrderModelList OrderModel 包含MemberName
            foreach (var item in orderList)
            {
                OrderModel om = new OrderModel(item);
                // 主要是为了显示 MemberName
                om.MemberName = _memberBLL.GetMemberById(item.MemberId).NickeName;

                //Address address = _addressBLL.GetAddressById(om.AddressId);
                //om.Phone = address.Phone;
                //om.Address = address.Province + "," + address.City + "," + address.County + "," + address.Detail;
                //om.Consignee = address.Consignee;

                orderModelList.Add(om);
            }

            sw.Stop();
            LogHelper.Log.Write("Home/Index: Foreach orderList " + sw.ElapsedMilliseconds + "ms");

            ViewData["OrderList"] = orderModelList;

            return View();
        }
    }
}