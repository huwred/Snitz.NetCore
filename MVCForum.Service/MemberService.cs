﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using X.PagedList;
using Microsoft.Extensions.Options;
using X.PagedList.Extensions;
using Microsoft.Data.SqlClient;
using SnitzCore.Service.Extensions;

namespace SnitzCore.Service
{
    public class MemberService : IMember
    {
        private readonly SnitzDbContext _dbContext;
        private readonly Dictionary<int, MemberRanking>? _rankings;
        private readonly ISnitzCookie _cookie;
        private readonly UserManager<ForumUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly string? _tableprefix;
        private readonly string? _memberprefix;

        public MemberService(SnitzDbContext dbContext,ISnitzCookie snitzcookie,UserManager<ForumUser> userManager,IHttpContextAccessor contextAccessor,IOptions<SnitzForums> config,RoleManager<IdentityRole> roleManager)
        {
            _dbContext = dbContext;
            _rankings = GetRankings();
            _cookie = snitzcookie;
            _userManager = userManager;
            _contextAccessor = contextAccessor;
            _tableprefix = config.Value.forumTablePrefix;
            _memberprefix = config.Value.memberTablePrefix;
            _roleManager = roleManager;
        }
        public async Task<List<Member>> GetUsersInRoleAsync(string roleName)
        {
            var usersInRole = new List<Member>();

            foreach (var user in _userManager.Users)
            {
                if (await _userManager.IsInRoleAsync(user, roleName))
                {
                    usersInRole.Add(GetByUsername(user.UserName));
                }
            }

            return usersInRole;
        }
        public Member? GetById(int? id)
        {
            if (id == null)
                return null;
            var member =  _dbContext.Members.AsNoTracking().OrderBy(m=>m.Id).First(m => m.Id == id);
            if(member == null) return null;

            var curruser = _userManager.FindByNameAsync(member.Name).Result;
            if (curruser != null)
            {
                IList<string> userroles = _userManager.GetRolesAsync(curruser).Result;
                member.Roles = userroles.ToList();
            }

            return member;
        }
        public Member? Get(int id)
        {
            return  _dbContext.Members.AsNoTracking().OrderBy(m=>m.Id).First(m => m.Id == id);
        }
        public async Task<Member?> GetById(ClaimsPrincipal user)
        {
            var userId = _userManager.GetUserId(user);
            if (userId != null)
            {
                var forumuser = await _userManager.FindByIdAsync(userId);
                if (forumuser != null) return GetById(forumuser.MemberId);
            }

            return null;
        }

        public string SHA256Hash(string password)
        {
            SHA256 sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(Encoding.ASCII.GetBytes(password));

            var stringBuilder = new StringBuilder();
            foreach (byte b in hash)
            {
                stringBuilder.Append($"{b:x2}");
            }
            return stringBuilder.ToString();
        }

        public IList<string> Roles(string username)
        {
            var curruser = _userManager.FindByNameAsync(username).Result;
            return curruser != null ? _userManager.GetRolesAsync(curruser).Result : new List<string>();
        }

        [Obsolete("Obsolete")]
        public bool ValidateMember(Member member, string password)
        {
            OldMembership? result = _dbContext.OldMemberships.OrderBy(m=>m.Id).FirstOrDefault(m => m.Id == member.Id);
            return result != null && CustomPasswordHasher.VerifyHashedPassword(result.Password,password);
        }

        public Member? Current()
        {
            if (_contextAccessor.HttpContext == null || _contextAccessor.HttpContext.User == null || _contextAccessor.HttpContext.User.Identity == null)
            {
                return null;
            }
            var user = _contextAccessor.HttpContext?.User.Identity?.Name;
            if (user != null) return GetByUsername(user);

            return null;
        }

        public async Task UpdatePostCount(int memberid)
        {
            var member = Get(memberid);
            if (member != null)
            {
                member.Posts += 1;
                member.Lastpostdate = DateTime.UtcNow.ToForumDateStr();
                member.Lastactivity = DateTime.UtcNow.ToForumDateStr();
                member.LastIp = _contextAccessor.HttpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString();
                _dbContext.Members.Update(member);
                await _dbContext.SaveChangesAsync();
            }
        }


        public Member Create(Member member)
        {
            var result = _dbContext.Members.Add(member);
            _dbContext.SaveChanges();
            return result.Entity;
        }
        public Member Create(Member member,List<KeyValuePair<string,object>> additionalFields)
        {
            var result = _dbContext.Members.Add(member);
            _dbContext.SaveChanges();
            _dbContext.Database.BeginTransaction();
            foreach (var additionalField in additionalFields)
            {
                if (additionalField.Key.ToUpper() == "DOB")
                {
                    var date = DateTime.Parse(additionalField.Value.ToString()!).ToString("yyyyMMdd");
                    _dbContext.Database.ExecuteSqlRaw("UPDATE " + _memberprefix + "MEMBERS SET M_"+additionalField.Key.ToUpper()+"=@Dob WHERE MEMBER_ID=@MemberId",new SqlParameter("Dob", date),new SqlParameter("MemberId", member.Id));
                }
                else
                {
                    _dbContext.Database.ExecuteSqlRaw("UPDATE " + _memberprefix + "MEMBERS SET M_"+additionalField.Key.ToUpper()+"=@FieldName WHERE MEMBER_ID=@MemberId ",new SqlParameter("FieldName", additionalField.Value),new SqlParameter("MemberId", member.Id));
                }
            }

            _dbContext.Database.ExecuteSqlRaw($"UPDATE "+_tableprefix+"TOTALS SET U_COUNT = U_COUNT + 1;");
            _dbContext.Database.CommitTransaction();

            return result.Entity;
        }
        public Dictionary<int, MemberRanking>? GetRankings()
        {
            return CacheProvider.GetOrCreate("Snitz.Rankings", () => Rankings(), TimeSpan.FromMinutes(10));

        }
        private Dictionary<int, MemberRanking>? Rankings()
        {
            Dictionary<int, MemberRanking> rankings = new Dictionary<int, MemberRanking>();
            try
            {
                foreach (var rank in _dbContext.MemberRanking)
                {
                    if (!rankings.ContainsKey(rank.Id))
                        rankings.Add(rank.Id, rank);
                }
            }
            catch (Exception)
            {
                //Supress any errors
            }
            return rankings;
        }
        public Member? GetMember(ClaimsPrincipal user)
        {
            return _dbContext.Members.SingleOrDefault(m=>m.Name == user.Identity!.Name);
        }

        public IEnumerable<MemberNamefilter> UserNameFilter()
        {
            return _dbContext.MemberNamefilter;
        }

        public void SetLastHere(ClaimsPrincipal? user)
        {
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
            {
                return;
            }
            var member = _dbContext.Members.SingleOrDefault(m=>m.Name == user.Identity.Name);
            if (member == null)
            {
                return;
            }
            //if there has been no activity for 10 minutes, reset the last login (LastLogin) date
            if (member.Lastactivity.FromForumDateStr().AddMinutes(20) < DateTime.UtcNow)
            {
                member.LastLogin = member.Lastactivity;
                if (member.LastLogin != null) _cookie.SetLastVisitCookie(member.LastLogin);
                member.LastIp = _contextAccessor.HttpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString();
            }            
            member.Lastactivity = DateTime.UtcNow.ToForumDateStr();
            _dbContext.Members.Update(member);
            _dbContext.SaveChanges();
        }

        public void Delete(Member newmember)
        {
            _dbContext.Members.Remove(newmember);
            _dbContext.SaveChanges();
        }

        public IEnumerable<Member> GetAll(bool isadmin)
        {
            return isadmin ? _dbContext.Members : _dbContext.Members.Where(m => m.Status == 1);
        }

        public IPagedList<Member> GetPagedMembers(bool isadmin,int pagesize = 20, int page = 1,string? sortcol = null,string? dir = "asc")
        {
            if (sortcol != null)
            {
                switch (sortcol)
                {
                    case "name" :
                        switch (dir)
                        {
                            case "asc" :
                                return isadmin ? _dbContext.Members.OrderBy(p => p.Name).ToPagedList(page, pagesize) : _dbContext.Members.Where(m=>m.Status == 1).OrderBy(p => p.Name).ToPagedList(page, pagesize);
                            default :
                                return isadmin ? _dbContext.Members.OrderByDescending(p => p.Name).ToPagedList(page, pagesize) : _dbContext.Members.Where(m=>m.Status == 1).OrderByDescending(p => p.Name).ToPagedList(page, pagesize);
                        }
                    case "lastpost" :
                        switch (dir)
                        {
                            case "asc" :
                                return isadmin ? _dbContext.Members.OrderBy(p => p.Lastpostdate).ToPagedList(page, pagesize) : _dbContext.Members.Where(m=>m.Status == 1).OrderBy(p => p.Lastpostdate).ToPagedList(page, pagesize);
                            default :
                                return isadmin ? _dbContext.Members.OrderByDescending(p => p.Lastpostdate).ToPagedList(page, pagesize) : _dbContext.Members.Where(m=>m.Status == 1).OrderByDescending(p => p.Lastpostdate).ToPagedList(page, pagesize);
                        }
                    case "lastvisit" :
                        switch (dir)
                        {
                            case "asc" :
                                return isadmin ? _dbContext.Members.OrderBy(p => p.LastLogin).ToPagedList(page, pagesize) : _dbContext.Members.Where(m=>m.Status == 1).OrderBy(p => p.LastLogin).ToPagedList(page, pagesize);
                            default :
                                return isadmin ? _dbContext.Members.OrderByDescending(p => p.LastLogin).ToPagedList(page, pagesize) : _dbContext.Members.Where(m=>m.Status == 1).OrderByDescending(p => p.LastLogin).ToPagedList(page, pagesize);
                        }
                    case "membersince" :
                        switch (dir)
                        {
                            case "asc" :
                                return isadmin ? _dbContext.Members.OrderBy(p => p.Created).ToPagedList(page, pagesize) : _dbContext.Members.Where(m=>m.Status == 1).OrderBy(p => p.Created).ToPagedList(page, pagesize);
                            default :
                                return isadmin ? _dbContext.Members.OrderByDescending(p => p.Created).ToPagedList(page, pagesize) : _dbContext.Members.Where(m=>m.Status == 1).OrderByDescending(p => p.Created).ToPagedList(page, pagesize);
                        }
                    case "posts" :
                        switch (dir)
                        {
                            case "asc" :
                                return isadmin ? _dbContext.Members.OrderBy(p => p.Posts).ToPagedList(page, pagesize) : _dbContext.Members.Where(m=>m.Status == 1).OrderBy(p => p.Posts).ToPagedList(page, pagesize);
                            default :
                                return isadmin ? _dbContext.Members.OrderByDescending(p => p.Posts).ToPagedList(page, pagesize) : _dbContext.Members.Where(m=>m.Status == 1).OrderByDescending(p => p.Posts).ToPagedList(page, pagesize);
                        }
                }
            }
            return isadmin ? _dbContext.Members.OrderByDescending(p => p.Posts).ToPagedList(page, pagesize) : _dbContext.Members.Where(m=>m.Status == 1).OrderByDescending(p => p.Posts).ToPagedList(page, pagesize);
        }

        public string? GetMemberName(int id)
        {
            return _dbContext.Members
                .OrderBy(m=>m.Id)
                .FirstOrDefault(m => m.Id == id)?.Name;
        }

        public IEnumerable<Member> GetByEmail(string email)
        {
            return _dbContext.Members
                .Where(m => m.Email == email);
        }

        public Member? GetByUsername(string username)
        {
            return _dbContext.Members.AsNoTracking()
                .Include(m=>m.Subscriptions).AsNoTracking()
                //in case multiple
                .FirstOrDefault(m=>m.Name == username);
        }

        public IPagedList<Member> GetByInitial(string initial,out int totalcount,int pagesize = 20, int page = 1)
        {
            var members = _dbContext.Members.Where(m=>m.Name.ToLower().StartsWith(initial.ToLower()));
            totalcount = members.Count();
            return members.ToPagedList(page, pagesize);
        }

        public IEnumerable<Member>? GetFilteredMembers(string searchQuery, string searchField)
        {
            switch (searchField)
            {
                case "1" :
                    return _dbContext.Members.Where(m=>m.Name.ToLower().Contains(searchQuery.ToLower()));
                case "2" :
                    return _dbContext.Members.Where(m=>m.Firstname != null && m.Firstname.ToLower().Contains(searchQuery.ToLower()));
                case "3" :
                    return _dbContext.Members.Where(m=>m.Lastname != null && m.Lastname.ToLower().Contains(searchQuery.ToLower()));
                case "4" :
                    var result = _dbContext.Members.AsEnumerable();
                    return result.Where(m => MemberRankTitle(m).ToLower().Contains(searchQuery.ToLower()));
                case "5" :
                    return _dbContext.Members.Where(m=> m.Email != null && m.Email.ToLower().Contains(searchQuery.ToLower()));
                default:
                    return null;
            }

        }

        public void Update(Member member)
        {

            _dbContext.Attach(member);
            _dbContext.Update(member);
            _dbContext.SaveChanges(true);
        }

        public int Create(string userName, string userEmail)
        {
            var member = new Member { Email = userEmail, Name = userName, Level = 1, Status = 1};
            var result = _dbContext.Members.Add(member);
            _dbContext.SaveChanges();
            return result.Entity.Id;
        }


        private string MemberRankTitle(Member member)
        {

            string mTitle = member.Title!;
            if (member.Status == 0 || member.Name == "n/a")
            {
                mTitle =  "Member Locked"; //ResourceManager.GetLocalisedString("tipMemberLocked", "Tooltip");// "Member Locked";
            }
            if (member.Name == "zapped")
            {
                mTitle = "Zapped Member"; //ResourceManager.GetLocalisedString("tipZapped", "Tooltip");// "Zapped Member";
            }

            var unused = new RankInfoHelper(member, ref mTitle!, member.Posts, _rankings);

            return mTitle;
        }

        [Obsolete("Obsolete")]
        public static bool VerifyHashedPassword(string hashedPassword, string password)
        {
             int PBKDF2IterCount = 1000; // default for Rfc2898DeriveBytes
             int PBKDF2SubkeyLength = 256 / 8; // 256 bits
             int SaltSize = 128 / 8; // 128 bits

            if (hashedPassword == null)
            {
                throw new ArgumentNullException(nameof(hashedPassword));
            }
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            byte[] hashedPasswordBytes = Convert.FromBase64String(hashedPassword);

            // Verify a version 0 (see comment above) password hash.

            if (hashedPasswordBytes.Length != (1 + SaltSize + PBKDF2SubkeyLength) || hashedPasswordBytes[0] != 0x00)
            {
                // Wrong length or version header.
                return false;
            }

            byte[] salt = new byte[SaltSize];
            Buffer.BlockCopy(hashedPasswordBytes, 1, salt, 0, SaltSize);
            byte[] storedSubkey = new byte[PBKDF2SubkeyLength];
            Buffer.BlockCopy(hashedPasswordBytes, 1 + SaltSize, storedSubkey, 0, PBKDF2SubkeyLength);

            byte[] generatedSubkey;
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, PBKDF2IterCount))
            {
                generatedSubkey = deriveBytes.GetBytes(PBKDF2SubkeyLength);
            }
            return ByteArraysEqual(storedSubkey, generatedSubkey);
        }
        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static bool ByteArraysEqual(byte[]? a, byte[]? b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            bool areSame = true;
            for (int i = 0; i < a.Length; i++)
            {
                areSame &= (a[i] == b[i]);
            }
            return areSame;
        }

        //[OutputCache(Duration = 600)]
        public IEnumerable<int> ForumSubscriptions()
        {
            var memberid = Current()?.Id;
            if (memberid.HasValue)
            {
                //return CacheProvider.GetOrCreate($"Subs_{memberid.Value}", ()=> GetForumSubscriptions(memberid.Value),TimeSpan.FromMinutes(10));
                return _dbContext.MemberSubscription.Where(s => s.MemberId == memberid).Select(s => s.ForumId).Distinct().OrderBy(o=>o);

            }

            return new List<int>();
        }
        private IEnumerable<int> GetForumSubscriptions(int memberid)
        {
            return _dbContext.MemberSubscription.Where(s => s.MemberId == memberid).Select(s => s.ForumId).Distinct().OrderBy(o=>o);
        }

        public IEnumerable<Member?> GetRecent(int max)
        {
            return _dbContext.Members.OrderByDescending(m=>m.Lastactivity).Take(max);
        }

        public async Task UpdateLastPost(int memberid)
        {
            var member = Get(memberid);
            if (member != null)
            {
                member.Lastpostdate = DateTime.UtcNow.ToForumDateStr();
                member.Lastactivity = DateTime.UtcNow.ToForumDateStr();
                member.LastIp = _contextAccessor.HttpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString();
                _dbContext.Members.Update(member);
                await _dbContext.SaveChangesAsync();
            }
        }

        public bool ZapMember(int memberid)
        {
            var member = _dbContext.Members.AsNoTracking().OrderBy(m=>m.Id).FirstOrDefault(m=>m.Id == memberid);
            _dbContext.SaveChanges();
            
            if (member != null)
            {
                var user = _userManager.FindByNameAsync(member.Name).Result;
                if (user != null)
                {
                    user.Email = "zapped@dummy.com";
                    _userManager.UpdateAsync(user);
                    var lockout = _userManager.SetLockoutEnabledAsync(user,true).Result;
                    if (lockout.Succeeded)
                    {
                        _userManager.SetLockoutEndDateAsync(user,DateTime.UtcNow.AddYears(10));
                    }
                    _userManager.UpdateSecurityStampAsync(user);
                }
                Member zappedMember = new()
                {
                    Id = memberid,
                    Name = "zapped",
                    Email = "zapped@dummy.com",
                    Posts = member.Posts,
                    Status = 0,
                    Title = "Zapped Member",
                    Created = member.Created
                };
                _dbContext.Update(zappedMember);
                _dbContext.SaveChanges();
                return true;
            }
            return false;
        }

        public bool HasRatedTopic(int topicid, int? memberid)
        {
            if(memberid == null)
            {
                return true;
            }
            return _dbContext.TopicRating.Where(t=>t.RatingsTopicId == topicid && t.RatingsBymemberId == memberid).Any();
        }

        public List<int>? AllowedForumIDs()
        {
            var context = _contextAccessor.HttpContext;
            if (context != null && context.Session != null && !context.Session.Keys.Contains("AllowedForums"))
            {
                context.Session.SetObject("AllowedForums", AllowedForums().Select(x => x).ToList());

            }
            if(context != null && context.Session != null && context.Session.Keys.Contains("AllowedForums"))
            {
                return context.Session.GetObject<List<int>>("AllowedForums");
            }
            return null;
        }

        public IEnumerable<int> AllowedForums()
        {
            List<int> allowedforums = new List<int>();
            var context = _contextAccessor.HttpContext;
            if (context != null && context.User == null)
            {
                return allowedforums;
            }
            else if (context != null && context.User != null)
            {
                //var forums = _dbContext.Forums("SELECT F.FORUM_ID,F.F_PRIVATEFORUMS,F.F_TOPICS,F.F_COUNT FROM " + db.ForumTablePrefix + "FORUM F");
                foreach (var forum in _dbContext.Forums)
                {
                    if (IsAllowed(forum.Id, _userManager.GetUserAsync(context.User).Result, forum.Privateforums))
                    {
                        if (!allowedforums.Contains(forum.Id))
                        {
                            allowedforums.Add(forum.Id);
                        }
                    }
                }
            }

            return allowedforums;
        }
  
        private bool IsAllowed(int forumid, ForumUser? user, ForumAuthType type)
        {

            if (user == null)
            {
                return type == ForumAuthType.All;
            }
            if (_userManager.IsInRoleAsync(user, "Administrator").Result)
            {
                return true;
            }
            if (type == ForumAuthType.All ||
                type == ForumAuthType.PasswordProtected ||
                type == ForumAuthType.Members ||
                type == ForumAuthType.MembersPassword)
                {
                    return true;
                }
            if (type == ForumAuthType.AllowedMembers ||
                type == ForumAuthType.AllowedMemberPassword ||
                type == ForumAuthType.AllowedMembersHidden )
                {
                    if (_userManager.IsInRoleAsync(user, "Forum_" + forumid).Result)
                    {
                        return true;
                    }

                        var exists = _dbContext.ForumAllowedMembers.Any(f=>f.ForumId == forumid && f.MemberId == user.MemberId);
                        return exists;
                }
            return false;
        }

    }
}
