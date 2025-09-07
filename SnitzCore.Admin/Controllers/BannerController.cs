﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using SnitzCore.Service;
using SnitzCore.Service.Extensions;

namespace SnitzCore.BackOffice.Controllers
{
    public class BannerController : Controller
    {
        private readonly IAdRotator _bannerService;

        public BannerController(IAdRotator bannerService)
        {
            _bannerService = bannerService;
        }

        public IActionResult Index()
        {
            var banners =  _bannerService.GetAds("Admin");
            return View(banners);
        }
        //[HttpPost]
        public IActionResult Save(Advert updatedAd)
        {
             _bannerService.Save(updatedAd);
            return Content("Banner updated");
        }
    }
}
