﻿using com.jiechengbao.entity;
using com.jiechengbao.Ibll;
using com.jiechengbao.wx.Global;
using com.jiechengbao.wx.Models;
using POPO.ActionFilter.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace com.jiechengbao.wx.Controllers
{
    [WhitespaceFilter]
    [ETag]
    public class HomeController : Controller
    {
        private ICategoryBLL _categoryBLL;
        private IGoodsBLL _goodsBLL;
        private IGoodsImagesBLL _goodsImagesBLL;
        private IGoodsCategoryBLL _goodsCategoryBLL;
        private IRulesBLL _rulesBLL;
        private IMemberBLL _memberBLL;
        private IAdvertisementBLL _advertisementBLL;
        public HomeController(ICategoryBLL categoryBLL,IGoodsBLL goodsBLL, 
            IGoodsImagesBLL goodsImagesBLL, IGoodsCategoryBLL goodsCategoryBLL, IRulesBLL rulesBLL, IMemberBLL memberBLL, IAdvertisementBLL advertisementBLL)
        {
            _categoryBLL = categoryBLL;
            _goodsBLL = goodsBLL;
            _goodsImagesBLL = goodsImagesBLL;
            _goodsCategoryBLL = goodsCategoryBLL;
            _rulesBLL = rulesBLL;
            _memberBLL = memberBLL;
            _advertisementBLL = advertisementBLL;
        }

        [IsLogin]
        public ActionResult Index()
        {    
            ViewData["CategoryList"] = _categoryBLL.GetAllCategory();
            return View();
        }

        [HttpPost]
        public PartialViewResult GoodsList(string category)
        {
            List<GoodsModel> gmList = new List<GoodsModel>();
            try
            {
                Member member = _memberBLL.GetMemberByOpenId(System.Web.HttpContext.Current.Session["member"].ToString());
                double discount = _rulesBLL.GetDiscountByVIP(member.Vip);

                if (string.IsNullOrEmpty(category) || category == "All")
                {
                    foreach (var item in _goodsBLL.GetGoodsByCountOrderByCreatedTime(10))
                    {
                        if (item.IsDeleted == true)
                        {
                            continue;
                        }

                        GoodsModel gm = new GoodsModel(item);
                        GoodsImage gi = _goodsImagesBLL.GetPictureByGoodsId(item.Id);
                        if (gi == null)
                        {
                            continue;
                        }
                        gm.PicturePath = gi.ImagePath;
                        gm.Discount = discount;
                        gmList.Add(gm);
                    }
                }
                else
                {
                    Category currentCategory = _categoryBLL.GetCategoryByCategoryNo(category);
                    List<GoodsCategory> goodsCategoryList = _goodsCategoryBLL.GetGoodsCategoryListByCategoryId(currentCategory.Id).ToList();
                    foreach (var item in goodsCategoryList)
                    {
                        Goods goods = _goodsBLL.GetGoodsById(item.GoodsId);
                        // 判断商品是否是被删除的
                        if (goods.IsDeleted == true)
                        {
                            continue;
                        }
                        GoodsModel gm = new GoodsModel(goods);
                        GoodsImage gi = _goodsImagesBLL.GetPictureByGoodsId(goods.Id);

                        if (gi == null)
                        {
                            continue;
                        }
                        gm.PicturePath = gi.ImagePath;
                        gm.Discount = discount;
                        gmList.Add(gm);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log.Write(ex.Message);
                LogHelper.Log.Write(ex.StackTrace);
            }
            ViewData["GoodsModelList"] = gmList;
            return PartialView();
        }

        [HttpPost]
        public ActionResult GetGoodsList(string categoryCode)
        {
            // 先取出当前vip等级会员折扣
            Member member = _memberBLL.GetMemberByOpenId(System.Web.HttpContext.Current.Session["member"].ToString());
            double discount = _rulesBLL.GetDiscountByVIP(member.Vip);

            List<GoodsModel> goodsList = new List<GoodsModel>();

            // 判断传递进来的categoryCode 是否为空

            // 如果为空 则搜索出所有没有标记为删除的Goods对象

            if (categoryCode == null || categoryCode == "All")
            {
                // 得到所有Goods对象之后  遍历  构造出 GoodsModel list
                foreach (var item in _goodsBLL.GetAllNoDeteledGoods())
                {
                    GoodsModel gm = new GoodsModel(item);

                    GoodsImage gi = _goodsImagesBLL.GetPictureByGoodsId(gm.Id);

                    if (gi == null)
                    {
                        continue;
                    }

                    gm.PicturePath = _goodsImagesBLL.GetPictureByGoodsId(gm.Id).ImagePath;
                    gm.Discount = discount;
                    goodsList.Add(gm);
                }
            }
            else
            {
                // 如果传进来的categoryCode 不为空

                // 则要根据这个categoryCode 找到对应的Category对象

                // 然后通过找到的category对象 得到  GoodsCategoryList

                Category category = _categoryBLL.GetCategoryByCategoryNo(categoryCode);
                List<GoodsCategory> goodsCategoryList = _goodsCategoryBLL.GetGoodsCategoryListByCategoryId(category.Id).ToList();

                // 最后再构造 GoodModelList
                foreach (var item in goodsCategoryList)
                {
                    Goods good = _goodsBLL.GetGoodsById(item.GoodsId);

                    GoodsModel gm = new GoodsModel(_goodsBLL.GetGoodsById(item.GoodsId));

                    GoodsImage gi = _goodsImagesBLL.GetPictureByGoodsId(item.GoodsId);
                    if (gi == null)
                    {
                        continue;
                    }

                    gm.PicturePath = _goodsImagesBLL.GetPictureByGoodsId(item.GoodsId).ImagePath;
                    gm.Discount = discount;
                    goodsList.Add(gm);
                }
            }

            // 由于这个方法需要与前端传递 List对象

            // 所以要将等到的 GoodsModel List 序列化为 json字符串 传递到前端

            string jsonResult = string.Empty;
            try
            {
                jsonResult = ObjToJson<List<GoodsModel>>(goodsList);
            }
            catch (Exception ex)
            {
                LogHelper.Log.Write(ex.Message);
                LogHelper.Log.Write(ex.StackTrace);
                throw;
            }
            return Content(jsonResult);
        }

        /// <summary>
        /// 获取广告列表  根据不同的分类
        /// </summary>
        /// <param name="categoryCode"></param>
        /// <returns></returns>
        [HttpPost]
        public PartialViewResult AdList(string categoryCode)
        {
            #region 注释           
            //// 上来先判断传进来的categoryCode 是否为空
            //if (string.IsNullOrEmpty(categoryCode))
            //{
            //    // 如果是 则查出前10个广告
            //    List<AdModel> AdList = new List<AdModel>();

            //    foreach (var item in _advertisementBLL.GetAllNotDeletedAdvertisements(10))
            //    {
            //        if (item.IsDeleted == true)
            //        {
            //            continue;
            //        }
            //        AdModel ad = new Models.AdModel();
            //        ad.AdName = item.AdName;
            //        ad.AdImagePath = item.AdImagePath;
            //        ad.AdDescription = item.AdDescription;

            //        AdList.Add(ad);
            //    }

            //    ViewData["AdModelList"] = AdList;
            //}
            //else
            //{
            //    Category category = _categoryBLL.GetCategoryByCategoryNo(categoryCode);
            //    if (category == null)
            //    {
            //        ViewData["AdModelList"] = null;
            //    }
            //    else
            //    {
            //        List<AdModel> adList = new List<Models.AdModel>();
            //        foreach (var item in _advertisementBLL.GetNotDeletedAdvertisementsByCategory(category.Id))
            //        {
            //            AdModel ad = new Models.AdModel();

            //            ad.AdName = item.AdName;
            //            ad.AdImagePath = item.AdImagePath;
            //            ad.AdDescription = item.AdDescription;

            //            adList.Add(ad);
            //        }

            //        ViewData["AdModelList"] = adList;
            //    }
            //}
            #endregion
            List<AdModel> AdModelList = new List<AdModel>();
            List<AdModel> IsReCommandList = new List<AdModel>();
            List<AdModel> NotReCommandList = new List<AdModel>();

            // 上来先判断 传进来的 cateogryCode 是否为空
            if (string.IsNullOrEmpty(categoryCode) || categoryCode == "All")
            {
                try
                {
                    #region 当 分类为空 时 获取广告列表
                    // 获取未删除的推荐广告
                    foreach (var item in _advertisementBLL.GetNotDeletedAndIsRecommandAd(5))
                    {
                        AdModel ad = new AdModel();
                        ad.AdName = item.AdName;
                        ad.AdCode = item.AdCode;
                        ad.AdDescription = item.AdDescription;
                        ad.AdImagePath = item.AdImagePath;
                        ad.IsRecommand = item.IsRecommend;

                        IsReCommandList.Add(ad);
                    }

                    // 获取未删除的非推荐广告
                    foreach (var item in _advertisementBLL.GetNotDeletedAndNotIsRecommandAd(10))
                    {
                        AdModel ad = new AdModel();
                        ad.AdName = item.AdName;
                        ad.AdCode = item.AdCode;
                        ad.AdDescription = item.AdDescription;
                        ad.AdImagePath = item.AdImagePath;
                        ad.IsRecommand = item.IsRecommend;

                        NotReCommandList.Add(ad);
                    }

                    // 构造AdModelList
                    int k = 0;
                    int nocount = NotReCommandList.Count;
                    for (int i = 0; i < IsReCommandList.Count; i++)
                    {
                        AdModelList.Add(IsReCommandList[i]);
                        if ((k += i) > nocount)
                        {
                            AdModelList.Add(NotReCommandList[k]);
                            if ((k += 1) > nocount)
                            {
                                AdModelList.Add(NotReCommandList[k]);
                            }
                        }
                    }

                    AdModelList.AddRange(NotReCommandList.Skip(k));

                    ViewData["AdModelList"] = AdModelList;

                    #endregion
                }
                catch (Exception ex)
                {
                    LogHelper.Log.Write(ex.Message);
                    LogHelper.Log.Write(ex.StackTrace);
                }

            }
            else
            {
                try
                {
                    #region 获取指定分类的广告列表
                    Category category = _categoryBLL.GetCategoryByCategoryNo(categoryCode);

                    if (category == null)
                    {
                        LogHelper.Log.Write("获取分类失败，category === null");
                    }
                    // 获取指定分类的未删除的推荐广告
                    List<Advertisement> temp_adList = new List<Advertisement>();
                    List<Advertisement> temp_no_adList = new List<Advertisement>();

                    temp_adList = _advertisementBLL.GetNotDeletedAndIsRecommandAdByCategory(category.Id).ToList();

                    temp_no_adList = _advertisementBLL.GetNotDeletedAndNotIsRecommandAdByCategory(category.Id).ToList();

                    foreach (var item in temp_adList)
                    {
                        AdModel ad = new AdModel();
                        ad.AdName = item.AdName;
                        ad.AdCode = item.AdCode;
                        ad.AdImagePath = item.AdImagePath;
                        ad.AdDescription = item.AdDescription;
                        ad.IsRecommand = item.IsRecommend;

                        IsReCommandList.Add(ad);
                    }

                    // 获取指定分类的未删除的非推荐广告
                    foreach (var item in temp_no_adList)
                    {
                        AdModel ad = new AdModel();
                        ad.AdName = item.AdName;
                        ad.AdCode = item.AdCode;
                        ad.AdImagePath = item.AdImagePath;
                        ad.AdDescription = item.AdDescription;
                        ad.IsRecommand = item.IsRecommend;

                        NotReCommandList.Add(ad);
                    }

                    // 构造 AdModelList
                    int k = 0;
                    int nocount = NotReCommandList.Count;
                    for (int i = 0; i < IsReCommandList.Count; i++)
                    {
                        LogHelper.Log.Write(IsReCommandList[i].AdName);
                        AdModelList.Add(IsReCommandList[i]);
                        if ((k += i) > nocount)
                        {
                            AdModelList.Add(NotReCommandList[k]);
                            if ((k += 1) > nocount)
                            {
                                AdModelList.Add(NotReCommandList[k]);
                            }
                        }
                    }

                    AdModelList.AddRange(NotReCommandList.Skip(k));
                    #endregion
                }
                catch (Exception ex)
                {
                    LogHelper.Log.Write(ex.Message);
                    LogHelper.Log.Write(ex.StackTrace);
                }
            }

            ViewData["AdModelList"] = AdModelList;
            return PartialView();
        }

        public ActionResult AdDetail(string adCode)
        {
            AdModel ad = new AdModel();

            Advertisement advertisement = _advertisementBLL.GetAdByAdCode(adCode);
            if (advertisement == null)
            {
                ViewBag.Advertisement = advertisement;
                return View();
            }

            ad.AdCode = advertisement.AdCode;
            ad.AdDescription = advertisement.AdDescription;
            ad.AdImagePath = advertisement.AdImagePath;
            ad.AdName = advertisement.AdName;
            ad.IsRecommand = advertisement.IsRecommend;

            ViewBag.Advertisement = ad;

            return View();
        }

        [NonAction]
        private string ObjToJson<T>(T data)
        {
            try
            {
                System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(data.GetType());
                using (MemoryStream ms = new MemoryStream())
                {
                    serializer.WriteObject(ms, data);
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log.Write(ex.Message);
                LogHelper.Log.Write(ex.StackTrace);
                return null;
            }
        }

        public ActionResult Detail(string code)
        {
            // 先取出当前vip等级会员折扣
            Member member = _memberBLL.GetMemberByOpenId(System.Web.HttpContext.Current.Session["member"].ToString());
            double discount = _rulesBLL.GetDiscountByVIP(member.Vip);

            GoodsModel gm = new GoodsModel(_goodsBLL.GetGoodsByCode(code));
            if (gm == null)
            {
                return View(gm);
            }
            GoodsImage gi = _goodsImagesBLL.GetPictureByGoodsId(gm.Id);
            gm.PicturePath = gi.ImagePath;
            gm.Discount = discount;

            return View(gm);
        }
    }
}