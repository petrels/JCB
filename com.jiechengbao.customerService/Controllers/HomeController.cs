﻿using com.jiechengbao.customerService.Models;
using com.jiechengbao.Ibll;
using com.jiechengbao.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;

namespace com.jiechengbao.customerService.Controllers
{
    public class HomeController : Controller
    {
        private IMemberBLL _memberBLL;
        public HomeController(IMemberBLL memberBLL)
        {
            _memberBLL = memberBLL;
        }

        /// <summary>
        /// 获取客户列表  返回的json
        /// 供 mui app 方便使用
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetContacts()
        {
            List<CustomerModel> cmList = new List<Models.CustomerModel>();

            foreach (var item in _memberBLL.GetAllNoDeletedMembers())
            {
                CustomerModel cm = new Models.CustomerModel();
                cm.HeadImage = item.HeadImage;
                cm.NickName = item.NickeName;
                cm.OpenId = item.OpenId;

                cmList.Add(cm);
            }

            DataContractJsonSerializer json = new DataContractJsonSerializer(cmList.GetType());

            string result = string.Empty;

            using (MemoryStream stream = new MemoryStream())
            {
                json.WriteObject(stream, cmList);
                result = Encoding.UTF8.GetString(stream.ToArray());
            }

            return Json(result);
        }
    }
}