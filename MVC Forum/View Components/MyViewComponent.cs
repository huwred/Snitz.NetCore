using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using MVCForum.ViewModels;
using SnitzCore.Data.Models;
using TagCloud;
using BbCodeFormatter.Processors;
using BbCodeFormatter;
using Microsoft.AspNetCore.Hosting;
using System.Text.RegularExpressions;

namespace MVCForum.View_Components
{
    public class MyViewViewComponent : ViewComponent
    {
        private readonly IForum _forumService;
        private readonly IMember _memberService;
        private readonly ICodeProcessor _bbCodeProcessor;
        private readonly IWebHostEnvironment _environment;

        public MyViewViewComponent(IMember memberservice,IForum forumservice,ICodeProcessor codeProcessor,IWebHostEnvironment environment)
        {
            _forumService = forumservice;
            _memberService = memberservice;
            _bbCodeProcessor = codeProcessor;
            _environment = environment;

        }
        public async Task<IViewComponentResult> InvokeAsync(string template, MyTopicsViewModel? model)
        {
            if (template == "TagCloud")
            {
                TagCloudSetting setting = new TagCloudSetting
                {
                    NumCategories = 15,
                    MaxCloudSize = 50
                };
                var stopwords = LoadStopWords();

                var phrases = _forumService.GetTagStrings(_memberService.ForumSubscriptions().ToList());
                var tagfree = new List<string>();
                Regex singleletters = new Regex(@"(?: |^|\(|\.|&)[A-Za-z0-9]{1,3}(?:$| |\.|,|\)|;)",RegexOptions.IgnoreCase | RegexOptions.Multiline);

                foreach (var phrase in phrases)
                {
                    string newphrase = _bbCodeProcessor.CleanCode(phrase);
                    newphrase = _bbCodeProcessor.StripCodeContents(newphrase);
                    newphrase = _bbCodeProcessor.StripTags(newphrase);
                    newphrase = _bbCodeProcessor.RemoveHtmlTags(newphrase);
                                          
                    if (stopwords.Any())
                    {
                        foreach (string word in stopwords )
                        {

                            Regex rgx = new Regex(@$"(?: |^|\(|\.)[{word}](?:$| |\.|,|\))",RegexOptions.IgnoreCase| RegexOptions.Multiline);
                            newphrase = rgx.Replace(newphrase, "");
                        }
                    }

                    tagfree.Add(singleletters.Replace(newphrase, ""));
                    

                }
                var vm = new TagCloudAnalyzer(setting)
                    
                    .ComputeTagCloud(tagfree)
                    .Shuffle();
                return await Task.FromResult((IViewComponentResult)View(template,vm));
            }

            if (template == "MyViewList")
            {
                return await Task.FromResult((IViewComponentResult)View(template,model.AllTopics));
            }
            if (template == "Posts")
            {
                return await Task.FromResult((IViewComponentResult)View(template,model));
            }
            return await Task.FromResult((IViewComponentResult)View(template));
        }
        private HashSet<string> LoadStopWords()
        {
            var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            if (culture == "nn" || culture == "nb")
            {
                culture = "no";
            }

            var path = Path.Combine(_environment.ContentRootPath, "App_Data\\stopwords-" + culture + ".txt");

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
