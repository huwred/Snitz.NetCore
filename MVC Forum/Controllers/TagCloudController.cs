using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
using SnitzCore.Data.Models;
using System.Collections.Generic;
using System.Globalization;
using System;
using System.IO;
using System.Linq;
using TagCloud;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data;
using BbCodeFormatter.Processors;
using Microsoft.AspNetCore.Hosting;
namespace MVCForum.Controllers
{
    public class TagCloudController : SnitzController
    {
        private readonly IForum _forumService;
        private readonly BbCodeProcessor _bbcodeProcessor;
        private readonly IWebHostEnvironment _environment;
        public TagCloudController(IMember memberService, ISnitzConfig config, IHtmlLocalizerFactory localizerFactory,SnitzDbContext dbContext,IHttpContextAccessor httpContextAccessor,
        IForum forumservice,BbCodeProcessor bbcodeprocessor,IWebHostEnvironment environment) : base(memberService, config, localizerFactory, dbContext,httpContextAccessor)
        {
            _forumService = forumservice;
            _bbcodeProcessor = bbcodeprocessor;
            _environment = environment;
        }
        //[OutputCache(Location = OutputCacheLocation.Server,Duration = 60,VaryByParam = "id")]
        public ActionResult Index(int id)
        {
            TagCloudSetting setting = new TagCloudSetting
            {
                NumCategories = 20, MaxCloudSize = 50
            };

            var phrases = id < 1 ? _forumService.GetTagStrings(_memberService.ForumSubscriptions().ToList()) : _forumService.GetTagStrings(new List<int>{id});
            var tagfree = new List<string>();

            foreach (var phrase in phrases)
            {
                string newphrase = _bbcodeProcessor.CleanCode(phrase);
                newphrase = _bbcodeProcessor.StripCodeContents(newphrase);
                newphrase = _bbcodeProcessor.StripTags(newphrase);
                newphrase = _bbcodeProcessor.RemoveHtmlTags(newphrase);
                tagfree.Add(newphrase);

            }
            var model = new TagCloudAnalyzer(setting)
                .ComputeTagCloud(tagfree)
                .Shuffle();
            return PartialView("_TagCloud",model);
        }



        public HashSet<string> LoadStopWords()
        {
            var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            if (culture == "nn" || culture == "nb")
            {
                culture = "no";
            }

            var path = Path.Combine(_environment.WebRootPath, "App_Data/stopwords-" + culture + ".txt");

            string logFile = "";
            if (System.IO.File.Exists(path))
            {
                logFile = System.IO.File.ReadAllText(path);

            }

            var wordList = logFile.Split(new string[] { Environment.NewLine, "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            return new HashSet<string>(wordList, StringComparer.OrdinalIgnoreCase);
        }

    }
}
