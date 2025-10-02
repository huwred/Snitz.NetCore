using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SnitzCore.Data.Extensions;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace SnitzCore.Service
{
    public class PrivateMessageService : IPrivateMessage
    {
        private readonly SnitzDbContext _dbContext;
        private readonly IMember _memberService;
        private readonly IEmailSender _mailSender;
        private readonly ISnitzConfig _config;
        public PrivateMessageService(SnitzDbContext dbContext,IMember memberService,IEmailSender mailSender,ISnitzConfig config)
        {
            _dbContext = dbContext;
            _memberService = memberService;
            _mailSender = mailSender;
            _config = config;
        }
        public PrivateMessage? GetById(int id)
        {
            return _dbContext.PrivateMessages.Include(p=>p.From).AsNoTracking().OrderBy(m=>m.Id).FirstOrDefault(pm=>pm.Id == id);
        }

        public IEnumerable<PrivateMessage> GetAll()
        {
            return _dbContext.PrivateMessages;
        }

        public IEnumerable<PrivateMessage> GetAll(int memberid)
        {
            return _dbContext.PrivateMessages.Where(pm=> (pm.FromId == memberid && pm.HideFrom == 0) || (pm.To == memberid && pm.HideTo == 0)).OrderByDescending(pm=>pm.SentDate);;
        }

        public IEnumerable<PrivateMessage> GetInbox(int memberid)
        {
            var blocked = GetBlockedMembers(memberid);

            return _dbContext.PrivateMessages.Include(p=>p.From).Where(pm=>pm.To == memberid && pm.HideTo == 0 && !EF.Constant(blocked).Contains(pm.To)).OrderByDescending(pm=>pm.SentDate);
        }

        public IEnumerable<PrivateMessage> GetOutbox(int memberid)
        {
            return _dbContext.PrivateMessages.Where(pm=>pm.FromId == memberid && pm.HideFrom == 0).OrderByDescending(pm=>pm.SentDate);;
        }

        public async Task Send(PrivateMessage pm)
        {
            _dbContext.PrivateMessages.Add(pm);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(int pmid, int memberid)
        {
            var privatemsg = _dbContext.PrivateMessages.SingleOrDefault(pm => pm.Id == pmid);

            if (privatemsg != null)
            {
                if (privatemsg.To == memberid)
                {
                    if (privatemsg.HideFrom == 1)
                    {
                        _dbContext.Remove(privatemsg);
                    }
                    else
                    {
                        privatemsg.HideTo = 1;
                        _dbContext.PrivateMessages.Update(privatemsg);
                    }
                    _dbContext.SaveChanges();
                }
                else if (privatemsg.FromId == memberid)
                {
                    if (privatemsg.HideTo == 1)
                    {
                        _dbContext.Remove(privatemsg);
                    }
                    else
                    {
                        privatemsg.HideFrom = 1;
                        _dbContext.PrivateMessages.Update(privatemsg);
                    }
                    _dbContext.SaveChanges();
                }
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteMany(IEnumerable<int> todelete, int memberid)
        {
            var privatemsgs = _dbContext.PrivateMessages.Where(pm => EF.Constant(todelete).Contains(pm.Id));
            try
            {
                _dbContext.RemoveRange(privatemsgs.Where(pm=>pm.To == memberid && pm.HideFrom == 1));
                _dbContext.RemoveRange(privatemsgs.Where(pm=>pm.FromId == memberid && pm.HideTo == 1));
                _dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                throw;
            }

            privatemsgs = _dbContext.PrivateMessages.Where(pm => EF.Constant(todelete).Contains(pm.Id));

            foreach (var privatemsg in privatemsgs)
            {
                if (privatemsg.To == memberid)
                {
                    if (privatemsg.HideFrom == 1)
                    {
                        
                        //_dbContext.PrivateMessages.Remove(privatemsg);
                    }
                    else
                    {
                        privatemsg.HideTo = 1;
                        _dbContext.PrivateMessages.Update(privatemsg);
                        _dbContext.SaveChanges();
                    }
                }
                else if (privatemsg.FromId == memberid)
                {
                    if (privatemsg.HideTo == 1)
                    {
                        //_dbContext.PrivateMessages.Remove(privatemsg);
                    }
                    else
                    {
                        privatemsg.HideFrom = 1;
                        _dbContext.PrivateMessages.Update(privatemsg);
                        _dbContext.SaveChanges();
                    }
                }
            }
            await _dbContext.SaveChangesAsync();
        }

        public Task Update(PrivateMessage pm)
        {
            _dbContext.PrivateMessages.Update(pm);
            _dbContext.SaveChanges();
            return Task.CompletedTask;
        }

        public Task UpdateContent(int pmid, string content)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PrivateMessage> GetLatestPMs(int memberid, int count)
        {
            return _dbContext.PrivateMessages.Where(pm=>pm.To == memberid && pm.HideTo == 0).OrderByDescending(pm=>pm.SentDate).Take(count);
        }

        public IEnumerable<PrivateMessageBlocklist>? GetBlocklist(int memberid)
        {
            try
            {
                return _dbContext.PrivateMessagesBlocklist.Where(pm=>pm.MemberId == memberid);
            }
            catch (Exception)
            {
                return null;
            }
            
        }

        public IEnumerable<int> GetBlocksByName(string membername)
        {
            return _dbContext.PrivateMessagesBlocklist.Where(pmb=>pmb.BlockedName == membername).Select(pmb=>pmb.MemberId);
        }

        public bool BlockListAdd(int memberId, string toblock)
        {
            try
            {
                var blockedmember = _memberService.GetByUsername(toblock);
                var isblocked = GetBlocksByName(toblock).Contains(memberId);
                if (blockedmember != null && !isblocked)
                {
                    var blockitem = new PrivateMessageBlocklist
                    {
                        MemberId = memberId, BlockedName = toblock, BlockedId = blockedmember.Id
                    };
                    _dbContext.PrivateMessagesBlocklist.Add(blockitem);
                    _dbContext.SaveChanges();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public void Create(PrivateMessage postmodel)
        {
            _dbContext.PrivateMessages.Add(postmodel);
            _dbContext.SaveChanges();

            if (postmodel?.Notify == 1  && (_config.GetIntValue("STREMAIL",0) == 1))
            {
                var tomember = _memberService.GetById(postmodel.To);
                BackgroundJob.Enqueue(() => _mailSender.SendPMNotification(tomember));
            }
        }


        public IEnumerable<int> GetBlockedMembers(int memberid)
        {
            return _dbContext.PrivateMessagesBlocklist.Where(pm=>pm.MemberId == memberid).Select(pm=>pm.BlockedId);
        }

        public IEnumerable<PrivateMessageListingModel> Find(int? curruser, string term, SearchIn searchIn, SearchFor phraseType, SearchDate searchByDays, int? memberId)
        {
            var messages = from pm in _dbContext.PrivateMessages
                join mfrom in _dbContext.Members on pm.FromId equals mfrom.Id 
                join mto in _dbContext.Members on pm.To equals mto.Id 
                where (pm.FromId == curruser || pm.To == curruser)
                select new PrivateMessageListingModel()
                {
                    Id = pm.Id,
                    Title = pm.Subject,
                    Description = pm.Message,
                    Sent = pm.SentDate.FromForumDateStr(),
                    Read = pm.Read == 1,
                    FromMemberId = pm.FromId,
                    ToMemberId = pm.To,
                    FromMemberName = mfrom == null ? "" : mfrom.Name,
                    ToMemberName = mto == null ? "" : mto.Name,
                };
            if (memberId.HasValue && memberId > 0)
            {
                messages = messages.Where(pm => pm.FromMemberId == memberId.Value || pm.ToMemberId == memberId.Value);
            }
            if (searchByDays != SearchDate.AnyDate)
            {
                messages = messages.Where(pm=>pm.Sent > DateTime.UtcNow.AddDays(-(int)searchByDays)); 
            }

            if (memberId.HasValue && string.IsNullOrWhiteSpace(term))
            {
                return messages;
            }
            var terms = term.ToUpper().Split(" ");
            switch (phraseType)
            {
                case SearchFor.AllTerms:
                    switch (searchIn)
                    {
                        case SearchIn.Message:
                            return messages.AsEnumerable().Where(pm=> terms.All(kw => pm.Description != null && pm.Description.ToUpper().Contains(kw)));
                        default:
                            return messages.AsEnumerable().Where(pm=> terms.All(kw => pm.Title.ToUpper().Contains(kw)));
                    }
                case SearchFor.AnyTerms:
                    switch (searchIn)
                    {
                        case SearchIn.Message:
                            return messages.AsEnumerable().Where(pm=> terms.Any(kw => pm.Description != null && pm.Description.ToUpper().Contains(kw)));
                        default:
                            return messages.AsEnumerable().Where(pm=> terms.Any(kw => pm.Title.ToUpper().Contains(kw)));
                    }
                default:
                    switch (searchIn)
                    {
                        case SearchIn.Message:
                            return messages.Where(pm=>pm.Description != null && pm.Description.Contains(term));
                        default:
                            return messages.Where(pm=>pm.Title.Contains(term));
                    }
            }
        }

        public void MarkRead(int id, int val)
        {
            var pm = GetById(id);
            if(pm != null)
            {
                pm.Read = val;
                _dbContext.PrivateMessages.Update(pm);
                _dbContext.SaveChanges();
            }

        }
    }
}
