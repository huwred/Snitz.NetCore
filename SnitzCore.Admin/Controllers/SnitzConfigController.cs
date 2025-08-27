using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SnitzCore.BackOffice.ViewModels;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;

namespace SnitzCore.BackOffice.Controllers
{
    [Authorize(Roles="Administrator")]
    public class SnitzConfigController : Controller
    {
        private readonly ISnitzConfig _config;
        private readonly SnitzDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        public SnitzConfigController(ISnitzConfig config,SnitzDbContext dbContext,RoleManager<IdentityRole> roleManager)
        {
            _config = config;
            _context = dbContext;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            //var vm = new 
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveSettings(IFormCollection form)
        {
            if (form.ContainsKey("_STRFORUMTITLE"))
            {
                SettingsHelpers.AddOrUpdateAppSetting("SnitzForums:strForumTitle", form["_STRFORUMTITLE"]);
            }
            if (form.ContainsKey("_STRCOPYRIGHT"))
            {
                SettingsHelpers.AddOrUpdateAppSetting("SnitzForums:strCopyright", form["_STRCOPYRIGHT"]);
            }
            if (form.ContainsKey("_STRFORUMDESC"))
            {
                SettingsHelpers.AddOrUpdateAppSetting("SnitzForums:strForumDescription", form["_STRFORUMDESC"]);
            }
            if (form.ContainsKey("_STRFORUMURL"))
            {
                SettingsHelpers.AddOrUpdateAppSetting("SnitzForums:strForumUrl", form["_STRFORUMURL"]);
            }
            return PartialView("SaveResult",SaveForm(form));
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveFeatures(IFormCollection form)
        {
            return PartialView("SaveResult",SaveForm(form));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveCaptcha(IFormCollection form)
        {
            return PartialView("SaveResult",SaveForm(form));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveBadword(IFormCollection form)
        {
            try
            {
                if (form["Word"][0] != null)
                {
                    var word = form["Word"][0];
                    var replace = form["ReplaceWith"][0];
                    if (word != null)
                    {
                        var newbadword = new Badword
                        {
                            Word = word,
                            ReplaceWith = replace
                        };

                        _context.Badwords.Add(newbadword);
                    }

                    _context.SaveChanges();
                }
                return Content("<script>location.reload();</script>");
            }
            catch (Exception e)
            {
                return PartialView("SaveResult",e.Message);
            }

        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateBadwords(AdminModeratorsViewModel model)
        {
            try
            {
                if (model.Badwords != null)
                    foreach (var badword in model.Badwords)
                    {
                        var exists = _context.Badwords.Find(badword.Id);
                        if (exists != null)
                        {
                            if (badword.IsDeleted)
                            {
                                _context.Remove(exists);
                            }
                            else if (exists.Word != badword.Word || exists.ReplaceWith != badword.ReplaceWith)
                            {
                                exists.Word = badword.Word;
                                exists.ReplaceWith = badword.ReplaceWith;
                                _context.Update(exists);
                            }
                        }
                    }

                _context.SaveChanges();

                return Content("<script>location.reload();</script>");
            }
            catch (Exception e)
            {
                return PartialView("SaveResult",e.Message);
            }

        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateUsernameFilter(AdminModeratorsViewModel model)
        {
            try
            {
                if (model.UserNamefilters != null)
                    foreach (var namefilter in model.UserNamefilters)
                    {
                        var exists = _context.MemberNamefilter.Find(namefilter.Id);
                        if (exists != null)
                        {
                            if (namefilter.IsDeleted)
                            {
                                _context.Remove(exists);
                            }
                            else if (!string.IsNullOrWhiteSpace(namefilter.Name) && exists.Name != namefilter.Name)
                            {
                                exists.Name = namefilter.Name;
                                _context.Update(exists);
                            }
                        }
                    }

                _context.SaveChanges();
                return PartialView("SaveResult", "UpdateUsernameFilter");
                //return Content("<script>location.reload();</script>");
            }
            catch (Exception e)
            {
                return PartialView("SaveResult",e.Message);
            }
        }
        
        [HttpPost]
        public IActionResult RankingConfig(RankingPost form)
        {
            try
            {
                var ranktype = _config.GetIntValue("STRSHOWRANK");
                if ((int)form.Type != ranktype)
                {
                    _config.SetValue("STRSHOWRANK", ((int)form.Type).ToString());
                }
                foreach (var rank in form.Ranks)
                {
                    var existingRank = _context.MemberRanking.Find(rank.Key);
                    if (existingRank != null)
                    {
                        existingRank.Title = rank.Value.Title;
                        existingRank.Posts = rank.Value.Posts;
                        existingRank.Image = rank.Value.Image;
                        existingRank.ImgRepeat = rank.Value.ImgRepeat;
                        if(rank.Key != 0)
                            _context.MemberRanking.Update(existingRank);
                    }

                }
                _context.SaveChanges(true);
                return PartialView("SaveResult", "Ranking saved");
            }
            catch (Exception e)
            {
                return PartialView("SaveResult",e.Message);
            }

        }

        public IActionResult SaveUsername(IFormCollection form)
        {
            try
            {
                if (form["Username"][0] != null)
                {
                    var name = form["Username"][0];
                    if (name != null)
                    {
                        var newusername = new MemberNamefilter
                        {
                            Name = name
                        };

                        _context.MemberNamefilter.Add(newusername);
                        _context.SaveChanges();
                    }

                }
                return Content("<script>location.reload();</script>");
            }
            catch (Exception e)
            {
                return PartialView("SaveResult", e.Message);
            }

        }

        public IActionResult RankingConfig()
        {
            var vm = new RankingViewModel(_context,_config);
            return PartialView("ManageRanking",vm);
        }
        
        public IActionResult SaveMemberSettings(IFormCollection form)
        {
            return PartialView("SaveResult",SaveForm(form));
        }
        private string SaveForm(IFormCollection form)
        {
            try
            {
                _context.Database.BeginTransaction();

                foreach (var formKey in form.Keys.Where(k => !k.StartsWith("_") && !k.StartsWith("X-")))
                {
                    var val = form[formKey][0];
                    if(formKey == "STRCAPTCHAOPERATORS"){
                        val = form[formKey].ToString().Replace(" ","");
                    }
                    var conf = _context.SnitzConfig.FirstOrDefault(f => f.Key == formKey);
                    if (conf != null)
                    {
                        if (conf.Value != val)
                        {
                            conf.Value = val;
                            _context.SnitzConfig.Update(conf);
                        }
                        _config.RemoveFromCache(formKey);
                    }
                    else
                    {
                        _context.SnitzConfig.Add(new SnitzConfig() { Id = 0, Key = formKey, Value = val });
                    }
                    if(formKey == "INTALLOWHIDEONLINE")
                    {
                        //Set-Up Role
                        if(val == "1" && !_roleManager.Roles.Any(r=>r.Name == "HiddenMembers"))
                        {
                            _roleManager.CreateAsync(new IdentityRole("HiddenMembers"));
                        }
                    }
                }
                _context.SaveChanges(true);
            }
            catch (Exception e)
            {
                _context.Database.RollbackTransaction();
                return e.Message;
            }
            finally
            {
                _context.Database.CommitTransaction();
            }

            return "Settings saved successfully";
        }

    }
}
