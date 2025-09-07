using Microsoft.AspNetCore.Hosting;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using SnitzCore.Service.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SnitzCore.Service
{
    public class BannerService : IAdRotator
    {

        private readonly string path = ""; //Path.Combine(HostingEnvironment.ApplicationPhysicalPath, @"App_Data/adrotator.xml");

        private readonly XmlHelper<AdCollection> _xmlHelper;

        public BannerService(IWebHostEnvironment env){

            path = Path.Combine(env.ContentRootPath, "App_Data","adrotator.xml");
            _xmlHelper = new XmlHelper<AdCollection>(path);
        }

        public AdCollection? GetAds(string location)
        {
            return CacheProvider.GetOrCreate<AdCollection>($"AdBanners_{location}", () => { 
                var ads = _xmlHelper.Read();
                if (location == "Admin")
                {
                    return ads;
                }
                return new AdCollection(){ Adverts = ads.Adverts.Where(a=>a.Keyword == location).ToArray() };
            },TimeSpan.FromHours(1));

        }

        public Advert? GetAd(IEnumerable<Advert> ads, int totalWeight)
        {
            // totalWeight is the sum of all brokers' weight

            int randomNumber = RandomGen2.Next(0, totalWeight);

            Advert? selectedAd = null;
            foreach (Advert ad in ads)
            {
                if (randomNumber < ad.Weight)
                {
                    selectedAd = ad;
                    break;
                }

                randomNumber = randomNumber - ad.Weight;
            }

            return selectedAd;
        }

        public void Save(AdCollection ads)
        {
            _xmlHelper.Write(ads);

            CacheProvider.Remove("AdBanners_Admin");
            CacheProvider.Remove("AdBanners_side");
            CacheProvider.Remove("AdBanners_top");
        }

        public void Save(Advert selectedAd)
        {
            Task.Run(() =>
            {
                var ads = _xmlHelper.Read().Adverts.ToList();
                var modified = ads.Where(u => u.Id.ToString() != selectedAd.Id.ToString());
                modified = modified.Concat(new[] { selectedAd });
                _xmlHelper.Write(new AdCollection(){Adverts = modified.ToArray()});
                CacheProvider.Remove("AdBanners_Admin");
                CacheProvider.Remove("AdBanners_side");
                CacheProvider.Remove("AdBanners_top");
            });
        }
        public void Add(Advert selectedAd)
        {
            var ads = _xmlHelper.Read().Adverts.ToList();

            var modified = ads.Concat(new[] { selectedAd }).ToList();

            _xmlHelper.Write(new AdCollection(){Adverts = modified.ToArray()});
            CacheProvider.Remove("AdBanners_Admin");
            CacheProvider.Remove("AdBanners_side");
            CacheProvider.Remove("AdBanners_top");

        }

        public void Delete(string id)
        {
            var ads = _xmlHelper.Read().Adverts.ToList();

            var modified = ads.Where(u => u.Id.ToString() != id).ToList();

            _xmlHelper.Write(new AdCollection(){Adverts = modified.ToArray()});
            CacheProvider.Remove("AdBanners_Admin");
            CacheProvider.Remove("AdBanners_side");
            CacheProvider.Remove("AdBanners_top");

        }


    }
    public static class RandomGen2
    {
        private static Random _global = new Random();
        [ThreadStatic]
        private static Random _local;

        public static int Next()
        {
            Random inst = _local;
            if (inst == null)
            {
                int seed;
                lock (_global) seed = _global.Next();
                _local = inst = new Random(seed);
            }
            return inst.Next();
        }

        public static int Next(int i, int totalWeight)
        {
            Random inst = _local;
            if (inst == null)
            {
                int seed;
                lock (_global) seed = _global.Next(i, totalWeight);
                _local = inst = new Random(seed);
            }
            return inst.Next(i, totalWeight);
        }
    }

}
