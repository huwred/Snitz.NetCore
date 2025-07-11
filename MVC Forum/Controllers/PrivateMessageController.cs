using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using MVCForum.ViewModels;
using MVCForum.ViewModels.PrivateMessage;
using SmartBreadcrumbs.Nodes;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using SnitzCore.Service.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MVCForum.Controllers
{
    [Authorize]
    public class PrivateMessageController : SnitzBaseController
    {
        private Member? _member;
        private readonly IPrivateMessage _pmService;

        public PrivateMessageController(IMember memberService, ISnitzConfig config, IHtmlLocalizerFactory localizerFactory,SnitzDbContext dbContext,IHttpContextAccessor httpContextAccessor,
            IPrivateMessage pmService) : base(memberService, config, localizerFactory, dbContext, httpContextAccessor)
        {
            _pmService = pmService;
        }

        public IActionResult Index()
        {
            _member = _memberService.GetMember(User);
            var inbox = _pmService.GetInbox(_member!.Id).Select(pm => new PrivateMessageListingModel()
            {
                Id = pm.Id,
                Title = pm.Subject,
                Description = pm.Message,
                Sent = pm.SentDate.FromForumDateStr(),
                Read = pm.Read == 1,
                FromMemberId = pm.From,
                ToMemberId = pm.To,
                FromMemberName = _memberService.GetMemberName(pm.From) ?? pm.From.ToString()
            });
            var profilePage = new MvcBreadcrumbNode("Index", "PrivateMessage", "mnuAccountPM");;
            ViewData["BreadcrumbNode"] = profilePage;
            
            var model = new PrivateMessageIndexModel()
            {
                MemberId = _member.Id,
                Inbox = inbox,
                Outbox = null
            };
            return View(model);
        }

        public IActionResult Inbox()
        {
            var profilePage = new MvcBreadcrumbNode("Index", "PrivateMessage", "mnuAccountPM");;
            ViewData["BreadcrumbNode"] = profilePage;
            
            _member = _memberService.GetMember(User);
            var inbox = _pmService.GetInbox(_member!.Id).Select(pm => new PrivateMessageListingModel()
            {
                Id = pm.Id,
                Title = pm.Subject,
                Description = pm.Message,
                Sent = pm.SentDate.FromForumDateStr(),
                Read = pm.Read == 1,
                FromMemberId = pm.From,
                ToMemberId = pm.To,
                FromMemberName = _memberService.GetMemberName(pm.From) ?? pm.From.ToString()
            });

            var model = new PrivateMessageIndexModel()
            {
                MemberId = _member.Id,
                 Inbox = inbox
            };
            return View("Index",model);
        }
        public IActionResult Outbox()
        {
            var profilePage = new MvcBreadcrumbNode("Index", "PrivateMessage", "mnuAccountPM");;
            ViewData["BreadcrumbNode"] = profilePage;

            _member = _memberService.GetMember(User);
            var outbox = _pmService.GetOutbox(_member!.Id).Select(pm => new PrivateMessageListingModel()
            {
                Id = pm.Id,
                Title = pm.Subject,
                Description = pm.Message,
                Sent = pm.SentDate.FromForumDateStr(),
                Read = pm.Read == 1,
                ToMemberId = pm.To,
                FromMemberId = pm.From,
                ToMemberName = _memberService.GetMemberName(pm.To) ?? pm.To.ToString()
            });
            var model = new PrivateMessageIndexModel()
            {
                MemberId = _member.Id,
                Outbox = outbox
            };
            return View("Index",model);
        }

        public IActionResult Delete(int pmid, int userid)
        {
            try
            {
                _pmService.Delete(pmid, userid).RunSynchronously();
                return Json(new { success = true, responseText= "Your message was deleted!"});
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText= e.Message});
            }

        }
        public IActionResult DeleteMany(PmDeleteViewModel form)
        {
            try
            {
                if (form.Delete != null) _pmService.DeleteMany(form.Delete, form.MemberId);
                return Json(new { success = true, responseText= "Your messages were deleted!"});
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText= e.Message});
            }

        }
        public IActionResult MarkRead(int id, int val)
        {
            _pmService.MarkRead(id, val);
            return Json(new { success = true, responseText= "Your messages were deleted!"});
        }
        public IActionResult Read(int id)
        {
            var message = _pmService.GetById(id);
            if(message == null)
            {
                return NotFound();
            }
            var pm = new PrivateMessageListingModel()
            {
                Id = message.Id,
                Title = message.Subject,
                Description = message.Message,
                Sent = message.SentDate.FromForumDateStr(),
                Read = message.Read == 1,
                FromMemberId = message.From,
                FromMemberName = _memberService.GetMemberName(message.From) ?? message.From.ToString()
            };
            message.Read = 1;
            _pmService.Update(message);

            return PartialView(pm);
        }

        public IActionResult Settings()
        {

            _member = _memberService.GetMember(User);
            var settings = new PrivateMessageSettingsModel
                {
                    EmailNotification = _member!.Pmemail == 1, 
                    SaveSentMessages = _member.Pmsavesent == 1,
                    RecievePM = _member.Pmreceive == 1,
                    BlockedList = _pmService.GetBlocklist(_member.Id)
                };

            return PartialView(settings);
        }

        [HttpPost]
        public IActionResult UpdateSettings(PrivateMessageSettingsModel settings)
        {
            _member = _memberService.GetMember(User);
            if (ModelState.IsValid)
            {
                _member!.Pmemail = settings.EmailNotification ? 1 : 0;
                _member.Pmreceive = settings.RecievePM ? 1 : 0;
                _member.Pmsavesent = (short)(settings.SaveSentMessages ? 1 : 0);
                _memberService.Update(_member);
            }
            var inbox = _pmService.GetInbox(_member!.Id).Select(pm => new PrivateMessageListingModel()
            {
                Id = pm.Id,
                Title = pm.Subject,
                Description = pm.Message,
                Sent = pm.SentDate.FromForumDateStr(),
                Read = pm.Read == 1,
                FromMemberId = pm.From,
                ToMemberId = pm.To,
                FromMemberName = _memberService.GetMemberName(pm.From) ?? pm.From.ToString()
            });
            settings = new PrivateMessageSettingsModel
            {
                EmailNotification = _member.Pmemail == 1, 
                SaveSentMessages = _member.Pmsavesent == 1,
                RecievePM = _member.Pmreceive == 1,
                BlockedList = _pmService.GetBlocklist(_member.Id)
            };
            var model = new PrivateMessageIndexModel()
            {
                Settings = settings,
                MemberId = _member.Id,
                Inbox = inbox,
                Outbox = null
            };
            return View("Index",model);
        }
        public IActionResult Create(int? id = null)
        {
            bool popup = false;
            string touser = "";
            if (id != null)
            {
                var name = _memberService.GetById(id)?.Name;
                if (name != null) touser = name;
                popup = true;
            }

            _member = _memberService.GetMember(User);
            return PartialView(new PrivateMessagePostModel(){SaveToSent = _member!.Pmsavesent == 1,To = touser,IsPopUp = popup});
        }
        public IActionResult Reply(int id)
        {

            var message = _pmService.GetById(id);
            var msgSent = message.SentDate.FromForumDateStr();
            _member = _memberService.GetMember(User);
            var header = $"\r\n\r\n\r\n----- Original Message -----\r\nSent: {msgSent.ToForumDisplay()} UTC\r\n";            
            var model = new PrivateMessagePostModel()
            {
                SaveToSent = _member?.Pmsavesent == 1, 
                Subject = "RE:" + message.Subject,
                Message = $"{header}\r\n {message.Message}",
                To = _memberService.GetMemberName(message.From)!,
                IsReply = true
            };
            return PartialView("Create",model);
        }
        public IActionResult Forward(int id)
        {

            var message = _pmService.GetById(id);
            var msgSent = message.SentDate.FromForumDateStr();
            _member = _memberService.GetMember(User)!;
            var header = $"\r\n\r\n\r\n----- Original Message -----\r\nFrom: {_memberService.GetMemberName(message.From)}\r\nSent: {msgSent.ToForumDisplay()} UTC\r\n";            
            var model = new PrivateMessagePostModel()
            {
                SaveToSent = _member.Pmsavesent == 1, 
                Subject = "FW:" + message.Subject,
                Message = $"{header}\r\n {message.Message}",
                IsReply = false
            };
            return PartialView("Create",model);
        }

        public new JsonResult AutoCompleteUsername(string term)
        {
            _member = _memberService.GetMember(User);
            if(_member == null)
            {
                return Json("");
            }
            IEnumerable<string> result = _memberService.GetAll(User.IsInRole("Administrator")).Where(r => r!.Name.ToLower().Contains(term.ToLower())).Select(m=>m!.Name);
            var blocked = _pmService?.GetBlocklist(_member!.Id)?.Select(l => l.BlockedName);
            if(blocked != null && blocked.Any()) {
                result = result.Where(x => !blocked.Contains(x) && x != _member.Name);
            }
            
            return Json(result);
        }

        public IActionResult Send(PrivateMessagePostModel postmodel)
        {
            _member = _memberService.GetMember(User);
            if (ModelState.IsValid)
            {
                //var recipients = postmodel.To.Split(";");
                if (postmodel.IncludeSig && !string.IsNullOrWhiteSpace(_member?.Signature))
                {
                    postmodel.Message += $"<br/><span>{_member.Signature}</span>";
                }

                if (_member != null)
                {
                    var tomember = _memberService.GetByUsername(postmodel.To);
                    _pmService.Create(new PrivateMessage()
                    {
                        From = _member.Id,
                        To = tomember!.Id,
                        Message = postmodel.Message,
                        Subject = postmodel.Subject,
                        SentDate = DateTime.UtcNow.ToForumDateStr(),
                        SaveSentMessage = (short)(postmodel.SaveToSent ? 1 : 0),
                    });

                }
                if (postmodel.IsPopUp)
                {
                    return Json(new { result = true });
                }                
            }
            else
            {
                if (postmodel.IsPopUp)
                {
                    return PartialView("Create",postmodel);
                }
            }

            var outbox = _pmService.GetOutbox(_member!.Id).Select(pm => new PrivateMessageListingModel()
            {
                Id = pm.Id,
                Title = pm.Subject,
                Description = pm.Message,
                Sent = pm.SentDate.FromForumDateStr(),
                Read = pm.Read == 1,
                ToMemberId = pm.To,
                FromMemberId = pm.From,
                ToMemberName = _memberService.GetMemberName(pm.To) ?? pm.To.ToString()
            });

            var model = new PrivateMessageIndexModel()
            {
                MemberId = _member.Id,
                Inbox = null,
                Outbox = outbox
            };
            return View("Index",model);
        }

        public IActionResult UpdateBlocklist(string blockmember)
        {
            var member = _memberService.GetMember(User);
            _pmService.BlockListAdd(member!.Id,blockmember);
            var inbox = _pmService.GetInbox(member.Id).Select(pm => new PrivateMessageListingModel()
            {
                Id = pm.Id,
                Title = pm.Subject,
                Description = pm.Message,
                Sent = pm.SentDate.FromForumDateStr(),
                Read = pm.Read == 1,
                FromMemberId = pm.From,
                ToMemberId = pm.To,
                FromMemberName = _memberService.GetMemberName(pm.From) ?? pm.From.ToString()
            });
            var settings = new PrivateMessageSettingsModel
            {
                EmailNotification = member.Pmemail == 1, 
                SaveSentMessages = member.Pmsavesent == 1,
                RecievePM = member.Pmreceive == 1,
                BlockedList = _pmService.GetBlocklist(member.Id)
            };
            var model = new PrivateMessageIndexModel()
            {
                Settings = settings,
                MemberId = member.Id,
                Inbox = inbox,
                Outbox = null
            };
            return View("Index",model);
        }

        public PartialViewResult SearchMessages(int id)
        {
            return PartialView("Search", new PMSearchViewModel());
        }
        [HttpPost]
        public PartialViewResult Search(PMSearchViewModel vm)
        {
            var curruser = _memberService.Current()?.Id;
            int? memberId = -1;
            if (!string.IsNullOrWhiteSpace(vm.MemberName))
                memberId = _memberService.GetByUsername(vm.MemberName)?.Id;

            var result = _pmService.Find(curruser,vm.Term,vm.SearchIn, vm.PhraseType,vm.SearchByDays, memberId);
            return PartialView("SearchResult", result);
        }

    }
}
