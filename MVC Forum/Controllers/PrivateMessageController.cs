using Microsoft.AspNetCore.Mvc;
using MVC_Forum.Models.Forum;
using MVCForum.Extensions;
using MVCForum.Models.PrivateMessage;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace MVCForum.Controllers
{
    [Authorize]
    public class PrivateMessageController : Controller
    {
        private Member _member;
        private readonly IPrivateMessage _pmService;
        private readonly IMember _memberService;

        public PrivateMessageController(IPrivateMessage pmService, IMember memberService)
        {
            _pmService = pmService;
            _memberService = memberService;

        }
        public IActionResult Index()
        {
            _member = _memberService.GetMember(User);
            var inbox = _pmService.GetInbox(_member.Id).Select(pm => new PrivateMessageListingModel()
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
                Inbox = inbox,
                Outbox = null
            };
            return View(model);
        }
        public IActionResult Inbox()
        {
            _member = _memberService.GetMember(User);
            var inbox = _pmService.GetInbox(_member.Id).Select(pm => new PrivateMessageListingModel()
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
            _member = _memberService.GetMember(User);
            var outbox = _pmService.GetOutbox(_member.Id).Select(pm => new PrivateMessageListingModel()
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
                var pm = _pmService.GetById(pmid);
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
                _pmService.DeleteMany(form.Delete,form.MemberId);
                return Json(new { success = true, responseText= "Your messages were deleted!"});
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText= e.Message});
            }

        }
        public IActionResult Read(int id)
        {
            var message = _pmService.GetById(id);
                
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

            return PartialView(pm);
        }

        public IActionResult Settings()
        {
            _member = _memberService.GetMember(User);
            var settings = new PrivateMessageSettingsModel
                {
                    EmailNotification = _member.Pmemail == 1, 
                    SaveSentMessages = _member.Pmsavesent == 1,
                    RecievePM = _member.Pmreceive == 1,
                    BlockedList = _pmService.GetBlocklist(_member.Id)
                };

            return PartialView(settings);
        }

        public IActionResult UpdateSettings(PrivateMessageSettingsModel settings)
        {
            _member = _memberService.GetMember(User);
            if (ModelState.IsValid)
            {
                _member.Pmemail = settings.EmailNotification ? 1 : 0;
                _member.Pmreceive = settings.RecievePM ? 1 : 0;
                _member.Pmsavesent = (short)(settings.SaveSentMessages ? 1 : 0);
                _memberService.Update(_member);
            }
            var inbox = _pmService.GetInbox(_member.Id).Select(pm => new PrivateMessageListingModel()
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
        public IActionResult Create()
        {
            _member = _memberService.GetMember(User);
            return PartialView(new PrivateMessagePostModel(){SaveToSent = _member.Pmsavesent == 1});
        }
        public IActionResult Reply(int id)
        {

            var message = _pmService.GetById(id);
            var msgSent = message.SentDate.FromForumDateStr();
            _member = _memberService.GetMember(User);
            var header = $"\r\n\r\n\r\n----- Original Message -----\r\nSent: {msgSent.ToForumDisplay()} UTC\r\n";            
            var model = new PrivateMessagePostModel()
            {
                SaveToSent = _member.Pmsavesent == 1, 
                Subject = "RE:" + message.Subject,
                Message = $"{header}\r\n {message.Message}",
                To = _memberService.GetMemberName(message.From),
                IsReply = true
            };
            return PartialView("Create",model);
        }
        public IActionResult Forward(int id)
        {

            var message = _pmService.GetById(id);
            var msgSent = message.SentDate.FromForumDateStr();
            _member = _memberService.GetMember(User);
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

        public JsonResult AutoCompleteUsername(string term)
        {
            _member = _memberService.GetMember(User);
            IEnumerable<string> result = _memberService.GetAll().Where(r => r.Name.ToLower().Contains(term.ToLower())).Select(m=>m.Name);
            var blocked = _pmService.GetBlocklist(_member.Id).Select(l => l.BlockedName);
            result = result.Where(x => !blocked.Contains(x) && x != _member.Name);
            return Json(result);
        }

        public IActionResult Send(PrivateMessagePostModel postmodel)
        {
            _member = _memberService.GetMember(User);
            var outbox = _pmService.GetOutbox(_member.Id).Select(pm => new PrivateMessageListingModel()
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
            _pmService.BlockListAdd(member.Id,blockmember);
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
    }
}
