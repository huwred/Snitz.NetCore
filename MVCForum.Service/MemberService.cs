using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace SnitzCore.Service
{
    public class MemberService : IMember
    {
        private readonly SnitzDbContext _dbContext;
        private readonly Dictionary<int, MemberRanking>? _rankings;
        private readonly ISnitzCookie _cookie;
        private readonly UserManager<ForumUser> _userManager;

        public MemberService(SnitzDbContext dbContext,ISnitzCookie snitzcookie,UserManager<ForumUser> userManager)
        {
            _dbContext = dbContext;
            _rankings = GetRankings();
            _cookie = snitzcookie;
            _userManager = userManager;

        }
        public Member? GetById(int? id)
        {
            if (id == null)
                return null;
            var member =  _dbContext.Members.AsNoTracking()
                .First(m => m.Id == id);

            var curruser = _userManager.FindByNameAsync(member.Name).Result;
            if (curruser != null)
            {
                IList<string> userroles = _userManager.GetRolesAsync(curruser).Result;
                member.Roles = userroles.ToList();
            }

            return member;
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
            OldMembership? result = _dbContext.OldMemberships.FirstOrDefault(m => m.Id == member.Id);
            return result != null && VerifyHashedPassword(result.Password,password);
        }

        public Member? Current()
        {
            throw new NotImplementedException();
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
                List<object> parameters = new List<object> { additionalField.Value };
                _dbContext.Database.ExecuteSqlRaw($"UPDATE FORUM_MEMBERS SET M_{additionalField.Key}=@0", parameters);
            }
            _dbContext.Database.CommitTransaction();

            return result.Entity;
        }
        public Dictionary<int, MemberRanking>? GetRankings()
        {
            Dictionary<int, MemberRanking> rankings = new Dictionary<int, MemberRanking>();
            var service = new InMemoryCache() { DoNotExpire = true };
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

            return service.GetOrSet("Snitz.Rankings", () => rankings);

        }

        public Member? GetMember(ClaimsPrincipal user)
        {
            return _dbContext.Members.SingleOrDefault(m=>m.Name == user.Identity!.Name);
        }

        public IEnumerable<MemberNamefilter> UserNameFilter()
        {
            return _dbContext.MemberNamefilter;
        }

        public void SetLastHere(ClaimsPrincipal user)
        {
            if (user.Identity is { IsAuthenticated: true })
            {
                var member = _dbContext.Members.SingleOrDefault(m=>m.Name == user.Identity.Name);
                if (member != null)
                {
                    var checkdate = member.Lastactivity != null ? DateTime.ParseExact(member.Lastactivity,"yyyyMMddHHmmss",CultureInfo.CurrentCulture) : DateTime.MinValue;
                    if (checkdate < DateTime.UtcNow.AddMinutes(-10))
                    {
                        member.Lastheredate = member.Lastactivity;
                        if (member.Lastheredate != null) _cookie.SetLastVisitCookie(member.Lastheredate);
                    }
                    member.Lastactivity = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                    _dbContext.SaveChanges();
                }
            }
        }

        public void Delete(Member newmember)
        {
            _dbContext.Members.Remove(newmember);
            _dbContext.SaveChanges();
        }

        public IEnumerable<Member> GetAll()
        {
            return _dbContext.Members;
        }

        public IPagedList<Member> GetPagedMembers(int pagesize = 20, int page = 1)
        {
            var members = _dbContext.Members.OrderByDescending(p => p.Posts);

            return members.ToPagedList(page, pagesize);

        }

        public string? GetMemberName(int id)
        {
            return _dbContext.Members
                .FirstOrDefault(m => m.Id == id)?.Name;
        }

        public IEnumerable<Member> GetByEmail(string email)
        {
            return _dbContext.Members
                .Where(m => m.Email == email);
        }

        public Member? GetByUsername(string username)
        {
            return _dbContext.Members.SingleOrDefault(m=>m.Name == username);
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
                    return _dbContext.Members.Where(m=>m.Email.ToLower().Contains(searchQuery.ToLower()));
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
    }
}
