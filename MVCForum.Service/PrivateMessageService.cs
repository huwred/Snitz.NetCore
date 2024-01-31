using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnitzCore.Service
{
    public class PrivateMessageService : IPrivateMessage
    {
        private readonly SnitzDbContext _dbContext;
        private readonly IMember _memberService;
        public PrivateMessageService(SnitzDbContext dbContext,IMember memberService)
        {
            _dbContext = dbContext;
            _memberService = memberService;
        }
        public PrivateMessage GetById(int id)
        {
            return _dbContext.PrivateMessages.First(pm=>pm.Id == id);
        }

        public IEnumerable<PrivateMessage> GetAll()
        {
            return _dbContext.PrivateMessages;
        }

        public IEnumerable<PrivateMessage> GetAll(int memberid)
        {
            return _dbContext.PrivateMessages.Where(pm=> (pm.From == memberid && pm.HideFrom == 0) || (pm.To == memberid && pm.HideTo == 0)).OrderByDescending(pm=>pm.SentDate);;
        }

        public IEnumerable<PrivateMessage> GetInbox(int memberid)
        {
            var blocked = GetBlockedMembers(memberid);

            return _dbContext.PrivateMessages.Where(pm=>pm.To == memberid && pm.HideTo == 0 && !blocked.Contains(pm.To)).OrderByDescending(pm=>pm.SentDate);
        }

        public IEnumerable<PrivateMessage> GetOutbox(int memberid)
        {
            return _dbContext.PrivateMessages.Where(pm=>pm.From == memberid && pm.HideFrom == 0).OrderByDescending(pm=>pm.SentDate);;
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
                    }
                }
                else if (privatemsg.From == memberid)
                {
                    if (privatemsg.HideTo == 1)
                    {
                        _dbContext.Remove(privatemsg);
                    }
                    else
                    {
                        privatemsg.HideFrom = 1;
                    }
                }
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteMany(IEnumerable<int> todelete, int memberid)
        {
            var privatemsgs = _dbContext.PrivateMessages.Where(pm => todelete.Contains(pm.Id));
            foreach (var privatemsg in privatemsgs)
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
                    }
                }
                else if (privatemsg.From == memberid)
                {
                    if (privatemsg.HideTo == 1)
                    {
                        _dbContext.Remove(privatemsg);
                    }
                    else
                    {
                        privatemsg.HideFrom = 1;
                    }
                }
            }
            await _dbContext.SaveChangesAsync();
        }

        public Task Update(PrivateMessage pm)
        {
            throw new NotImplementedException();
        }

        public Task UpdateContent(int pmid, string content)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PrivateMessage> GetLatestPMs(int memberid, int count)
        {
            return _dbContext.PrivateMessages.Where(pm=>pm.To == memberid && pm.HideTo == 0).OrderByDescending(pm=>pm.SentDate).Take(count);
        }

        public IEnumerable<PrivateMessageBlocklist> GetBlocklist(int memberid)
        {
            return _dbContext.PrivateMessagesBlocklist.Where(pm=>pm.MemberId == memberid);
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
        }


        public IEnumerable<int> GetBlockedMembers(int memberid)
        {
            return _dbContext.PrivateMessagesBlocklist.Where(pm=>pm.MemberId == memberid).Select(pm=>pm.BlockedId);
        }
    }
}
