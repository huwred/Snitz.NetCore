using BbCodeFormatter;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MVCForum.ViewModels;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TagCloud;
using X.PagedList;

namespace MVCForum.View_Components
{
    public class BlogViewComponent : ViewComponent
    {
        private readonly IForum _forumService;
        private readonly ICodeProcessor _bbcodeProcessor;
        private readonly IWebHostEnvironment _environment;
        public BlogViewComponent(IForum forumService,ICodeProcessor bbcodeProcessor,IWebHostEnvironment webHostEnvironment)
        {
            _forumService = forumService;
            _bbcodeProcessor = bbcodeProcessor;
            _environment = webHostEnvironment;
            // Constructor logic if needed
        }
        public async Task<IViewComponentResult> InvokeAsync(string template, int? id = null)
        {
            if(template == "List" && id != null && id != 0){
                var forum = _forumService.GetWithPosts(id.Value);
                PagedList<Post> result = new PagedList<Post>(forum.Posts!,1,50); //forum.Posts(50, 1, User, WebSecurity.CurrentUserId, 120);

                ForumViewModel vm = new()
                {
                    Forum = forum, 
                    Topics = result.ToList()
                };
                vm.Id = forum.Id;
                vm.Forum = forum;
                vm.PageSize = 50;
                vm.StickyTopics = null;
                vm.TotalRecords = result.TotalItemCount;
                int pagecount = Convert.ToInt32(result.PageCount);
                vm.PageCount = pagecount;
                vm.Page = 1;
                return await Task.FromResult((IViewComponentResult)View(template,vm));
            }else if (template == "TagCloud" && id != null && id != 0)
            {
                TagCloudSetting setting = new TagCloudSetting
                {
                    NumCategories = 15,
                    MaxCloudSize = 50
                };
                var stopwords = LoadStopWords();

                var forum = _forumService.GetWithPosts(id.Value);

                var phrases =  forum.Posts.Select(p=>p.Content);
                var tagfree = new List<string>();

                Regex singleletters = new Regex(@"(?: |^|\(|\.|&|\#)[A-Za-z0-9 ]{1,3}(?:$| |\.|,|\)|;)",RegexOptions.IgnoreCase | RegexOptions.Multiline);

                foreach (var phrase in phrases)
                {
                    string newphrase = _bbcodeProcessor.CleanCode(phrase);
                    newphrase = _bbcodeProcessor.StripCodeContents(newphrase);
                    newphrase = _bbcodeProcessor.StripTags(newphrase);
                    newphrase = _bbcodeProcessor.RemoveHtmlTags(newphrase);
                    newphrase = singleletters.Replace(newphrase, " ");                     
                    if (stopwords.Any())
                    {
                        foreach (string word in stopwords )
                        {

                            Regex rgx = new Regex(@$"(?: |^|\(|\.|\#)({word})(?:$| |\.|,|\))",RegexOptions.IgnoreCase| RegexOptions.Multiline);
                            newphrase = rgx.Replace(newphrase, " ");
                        }
                    }

                    tagfree.Add(newphrase);
                    

                }
                var vm = new TagCloudAnalyzer(setting)
                    .ComputeTagCloud(tagfree)
                    .Shuffle();
                return await Task.FromResult((IViewComponentResult)View(template,vm));
            }
            // You can add logic here to handle different templates or models
            return Content(string.Empty);
        }
        private HashSet<string> LoadStopWords()
        {
            var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            if (culture == "nn" || culture == "nb")
            {
                culture = "no";
            }

            var path = Path.Combine(_environment.ContentRootPath, "App_Data","stopwords-" + culture + ".txt");
            string logFile = "";
            if (System.IO.File.Exists(path))
            {
                logFile = System.IO.File.ReadAllText(path);

            }

            var wordList = logFile.Split(new string[] { Environment.NewLine, "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            return new HashSet<string>(wordList, StringComparer.OrdinalIgnoreCase);
        }

        // Additional methods for handling specific templates can be added here
    }
}
