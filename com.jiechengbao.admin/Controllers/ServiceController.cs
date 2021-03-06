﻿using com.jiechengbao.admin.Models;
using com.jiechengbao.entity;
using com.jiechengbao.Ibll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace com.jiechengbao.admin.Controllers
{
    public class ServiceController : Controller
    {
        private IServiceBLL _serviceBLL;
        private IMemberBLL _memberBLL;
        private IServiceConsumePasswordBLL _serviceConsumePasswordBLL;

        public ServiceController(IServiceBLL serviceBLL, IServiceConsumePasswordBLL serviceConsumePasswordBLL,
            IMemberBLL memberBLL)
        {
            _serviceBLL = serviceBLL;
            _memberBLL = memberBLL;
            _serviceConsumePasswordBLL = serviceConsumePasswordBLL;
        }

        public ActionResult List()
        {
            List<MyService> msList = new List<MyService>();
            List<Member> memberList = new List<Member>();
            List<MyServiceModel> msmList = new List<MyServiceModel>();

            memberList = _memberBLL.GetMemberswithSpecifiedCount(50).ToList();
            foreach (var item in memberList)
            {
                msList = _serviceBLL.GetMyServiceByMemberId(item.Id).ToList();

                foreach (var ms in msList)
                {
                    MyServiceModel msm = new MyServiceModel();
                    msm.CreateTime = ms.CreatedTime;
                    msm.CurrentCount = ms.CurrentCount;
                    msm.MemberImage = item.HeadImage;
                    msm.MemberName = item.NickeName;
                    msm.ServiceName = ms.GoodsName;
                    msm.TotalCount = ms.TotalCount;

                    msmList.Add(msm);
                }
            }

            ViewData["MyServiceModelList"] = msmList;
            return View();
        }

        public ActionResult PasswordSetting()
        {
            ServiceConsumePassword scp = _serviceConsumePasswordBLL.GetServicePassword();
            string currentpassword = string.Empty;
            if (scp != null)
            {
                currentpassword = scp.Password;
            }
            ViewBag.password = currentpassword;
            return View();
        }
        [HttpPost]
        public ActionResult UpdatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return Json("False", JsonRequestBehavior.AllowGet);
            }
            ServiceConsumePassword scp = _serviceConsumePasswordBLL.GetServicePassword();

            if (scp != null)
            {
                scp.Password = password;
                scp.CreatedTime = DateTime.Now;

                if (_serviceConsumePasswordBLL.Update(scp))
                {
                    return Json("True", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("False", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                ServiceConsumePassword scp_new = new ServiceConsumePassword();
                scp_new.Id = Guid.NewGuid();
                scp_new.IsDeleted = false;
                scp_new.Password = password;
                scp_new.DeletedTime = DateTime.MinValue.AddHours(8);
                scp_new.CreatedTime = DateTime.Now;

                if (_serviceConsumePasswordBLL.Add(scp_new))
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
}