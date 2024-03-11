using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SnitzCore.BackOffice.ViewModels;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;

namespace SnitzCore.BackOffice.Controllers
{
    [Authorize(Roles="Administrator")]
    public class SnitzConfigController : Controller
    {
        private readonly ISnitzConfig _config;
        private readonly SnitzDbContext _context;
        public SnitzConfigController(ISnitzConfig config,SnitzDbContext dbContext)
        {
            _config = config;
            _context = dbContext;
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
                return PartialView("SaveResult","Badword saved");
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
                            if (exists.Word != badword.Word || exists.ReplaceWith != badword.ReplaceWith)
                            {
                                exists.Word = badword.Word;
                                exists.ReplaceWith = badword.ReplaceWith;
                                _context.Update(exists);
                            }
                        }
                    }

                _context.SaveChanges();

                return PartialView("SaveResult","Badwords updated");
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
                return PartialView("SaveResult", "Username saved");
            }
            catch (Exception e)
            {
                return PartialView("SaveResult", e.Message);
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
                            if (exists.Name != namefilter.Name)
                            {
                                exists.Name = namefilter.Name;
                                _context.Update(exists);
                            }
                        }
                    }

                _context.SaveChanges();

                return PartialView("SaveResult","Username filters updated");
            }
            catch (Exception e)
            {
                return PartialView("SaveResult",e.Message);
            }
        }
        public IActionResult RankingConfig()
        {
            var vm = new RankingViewModel(_context,_config);
            return PartialView("ManageRanking",vm);
        }
        [HttpPost]
        public IActionResult RankingConfig(RankingViewModel model)
        {
            
            return PartialView("SaveResult", "Ranking saved");
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

                foreach (var formKey in form.Keys.Where(k => !k.StartsWith("_")))
                {
                    var val = form[formKey][0];
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
                        if (val != "0" && val != "")
                        {
                            _context.SnitzConfig.Add(new SnitzConfig() { Id = 0, Key = formKey, Value = val });
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
