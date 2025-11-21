using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using SnitzCore.Data.ViewModels;
using SnitzCore.Service.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;
using X.PagedList.Extensions;

namespace SnitzCore.Service
{
    public class MemberService : IMember
    {
        private readonly SnitzDbContext _dbContext;
        private readonly Dictionary<int, MemberRanking>? _rankings;
        private readonly ISnitzCookie _cookie;
        private readonly UserManager<ForumUser> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly string? _tableprefix;
        private readonly string? _memberprefix;

        public MemberService(SnitzDbContext dbContext,ISnitzCookie snitzcookie,UserManager<ForumUser> userManager,IHttpContextAccessor contextAccessor,IOptions<SnitzForums> config)
        {
            _dbContext = dbContext;
            _rankings = GetRankings();
            _cookie = snitzcookie;
            _userManager = userManager;
            _contextAccessor = contextAccessor;
            _tableprefix = config.Value.forumTablePrefix;
            _memberprefix = config.Value.memberTablePrefix;
        }
        
        /// <summary>
        /// Retrieves a list of users who are assigned to the specified role.
        /// </summary>
        /// <remarks>This method queries all users and checks their role membership asynchronously. It may
        /// involve multiple database calls, depending on the implementation of the underlying user manager.</remarks>
        /// <param name="roleName">The name of the role to search for. This value cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Member"/>
        /// objects representing the users in the specified role. If no users are found, the list will be empty.</returns>
        public async Task<List<Member>> GetUsersInRoleAsync(string roleName)
        {
            var usersInRole = new List<Member>();

            foreach (var user in _userManager.Users.ToList())
            {
                if (await _userManager.IsInRoleAsync(user, roleName))
                {
                    usersInRole.Add(GetByUsername(user.UserName));
                }
            }

            return usersInRole;
        }

        /// <summary>
        /// Retrieves a member by their unique identifier.
        /// </summary>
        /// <remarks>This method performs a database query to retrieve the member with the specified
        /// identifier.  If the member exists, their roles are also retrieved and populated in the <see
        /// cref="Member.Roles"/> property.</remarks>
        /// <param name="id">The unique identifier of the member to retrieve. Must not be null.</param>
        /// <returns>The <see cref="Member"/> object corresponding to the specified identifier, including its roles,  or <see
        /// langword="null"/> if no member with the given identifier exists.</returns>
        public Member? GetById(int? id)
        {
            if (id == null)
                return null;
            var member =  _dbContext.Members.AsNoTracking().OrderBy(m=>m.Id).FirstOrDefault(m => m.Id == id);
            if(member == null) return null;

            var curruser = _userManager.FindByNameAsync(member.Name).Result;
            if (curruser != null)
            {
                IList<string> userroles = _userManager.GetRolesAsync(curruser).Result;
                member.Roles = userroles.ToList();
            }

            return member;
        }

        /// <summary>
        /// Retrieves a member with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the member to retrieve.</param>
        /// <returns>The <see cref="Member"/> object with the specified identifier, or <see langword="null"/> if no such member
        /// exists.</returns>
        public Member? Get(int id)
        {
            return  _dbContext.Members.AsNoTracking().OrderBy(m=>m.Id).First(m => m.Id == id);
        }

        /// <summary>
        /// Retrieves a <see cref="Member"/> object associated with the specified user.
        /// </summary>
        /// <remarks>This method uses the provided <see cref="ClaimsPrincipal"/> to determine the user's
        /// identity and retrieve their associated member. If the user cannot be resolved or does not have an associated
        /// member, the method returns <see langword="null"/>.</remarks>
        /// <param name="user">The <see cref="ClaimsPrincipal"/> representing the user whose associated member is to be retrieved.</param>
        /// <returns>The <see cref="Member"/> object associated with the specified user, or <see langword="null"/> if the user is
        /// not found  or does not have an associated member.</returns>
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

        /// <summary>
        /// Computes the SHA-256 hash of the specified password and returns it as a hexadecimal string.
        /// </summary>
        /// <param name="password">The input password to hash. Cannot be <see langword="null"/> or empty.</param>
        /// <returns>A hexadecimal string representation of the SHA-256 hash of the input password.</returns>
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

        /// <summary>
        /// Retrieves the roles associated with the specified user.
        /// </summary>
        /// <remarks>This method performs a synchronous operation by awaiting asynchronous calls
        /// internally.  It is recommended to use this method in scenarios where blocking is acceptable.</remarks>
        /// <param name="username">The username of the user whose roles are to be retrieved. Cannot be null or empty.</param>
        /// <returns>A list of role names assigned to the user. Returns an empty list if the user does not exist or has no roles.</returns>
        public IList<string> Roles(string username)
        {
            var curruser = _userManager.FindByNameAsync(username).Result;
            return curruser != null ? _userManager.GetRolesAsync(curruser).Result : new List<string>();
        }

        /// <summary>
        /// Validates the provided password for the specified member.
        /// </summary>
        /// <remarks>This method checks the validity of a member's password by first determining whether
        /// the member exists in the database. If the member is found, the password is validated against the stored
        /// hash. If the member does not exist, the method attempts to validate the password against legacy membership
        /// data.</remarks>
        /// <param name="member">The member whose password is being validated. Cannot be <see langword="null"/>.</param>
        /// <param name="password">The password to validate. Cannot be <see langword="null"/> or empty.</param>
        /// <returns>A <see cref="MigratePassword"/> value indicating the result of the validation: <list type="bullet">
        /// <item><description><see cref="MigratePassword.Valid"/> if the password is correct.</description></item>
        /// <item><description><see cref="MigratePassword.InvalidPassword"/> if the password is
        /// incorrect.</description></item> <item><description><see cref="MigratePassword.NoMember"/> if the member does
        /// not exist in the database.</description></item> </list></returns>
        public MigratePassword ValidateMember(Member member, string password)
        {
            try
            {
                OldMembership? result = _dbContext.OldMemberships.OrderBy(m=>m.Id).FirstOrDefault(m => m.Id == member.Id);
                if(result == null)
                {
                    //no old .net membership so must be using Member.Password
                    var memberid = member.Id;
                    var oldpass = _dbContext.Database.SqlQuery<string>($"SELECT M_PASSWORD FROM FORUM_MEMBERS WHERE MEMBER_ID = {memberid}");
                    var strencoded = SHA256Hash(password);
                    if (oldpass != null && oldpass.Count() > 0)
                    {
                        return strencoded == oldpass.ToList().First() ? MigratePassword.Valid : MigratePassword.InvalidPassword;
                    }
                    return MigratePassword.NoMember;
                }
                if(result != null) {
                    return CustomPasswordHasher.VerifyHashedPassword(result.Password,password) ? MigratePassword.Valid : MigratePassword.InvalidPassword;
                }
            }
            catch (Exception)
            {
                    //no old .net membership so must be using Member.Password
                    var memberid = member.Id;
                    var oldpass = _dbContext.Database.SqlQuery<string>($"SELECT M_PASSWORD FROM FORUM_MEMBERS WHERE MEMBER_ID = {memberid}");
                    var strencoded = SHA256Hash(password);
                    if (oldpass != null && oldpass.Count() > 0)
                    {
                        return strencoded == oldpass.ToList().First() ? MigratePassword.Valid : MigratePassword.InvalidPassword;
                    }
                    return MigratePassword.NoMember;
            }

            return MigratePassword.NoMember;

        }

        /// <summary>
        /// Retrieves the currently authenticated user based on the HTTP context.
        /// </summary>
        /// <remarks>This method attempts to determine the currently authenticated user by accessing the 
        /// HTTP context and retrieving the username from the user's identity. If the HTTP context,  user, or identity
        /// is null, the method returns <see langword="null"/>.</remarks>
        /// <returns>The <see cref="Member"/> object representing the currently authenticated user, or  <see langword="null"/> if
        /// no user is authenticated or the HTTP context is unavailable.</returns>
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

        /// <summary>
        /// Updates the post count and activity details for the specified member.
        /// </summary>
        /// <remarks>This method increments the member's post count by one and updates their last post
        /// date, last activity date,  and last known IP address. If the specified member does not exist, the method
        /// performs no action.</remarks>
        /// <param name="memberid">The unique identifier of the member whose post count is to be updated.</param>
        /// <returns></returns>
        public async Task UpdatePostCount(int memberid)
        {
            var member = _dbContext.Members.Find(memberid);
            if (member != null)
            {
                member.Posts += 1;
                member.Lastpostdate = DateTime.UtcNow.ToForumDateStr();
                member.Lastactivity = DateTime.UtcNow.ToForumDateStr();
                member.LastIp = _contextAccessor.HttpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString();
                _dbContext.Update(member);
                await _dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Adds a new member to the database and saves the changes.
        /// </summary>
        /// <remarks>This method persists the provided <see cref="Member"/> object to the database and
        /// returns the entity  with any updates made by the database, such as generated primary keys.</remarks>
        /// <param name="member">The <see cref="Member"/> object to add to the database. Cannot be <see langword="null"/>.</param>
        /// <returns>The <see cref="Member"/> object that was added, including any database-generated values.</returns>
        public Member Create(Member member)
        {
            var result = _dbContext.Members.Add(member);
            _dbContext.SaveChanges();
            return result.Entity;
        }

        /// <summary>
        /// Creates a new member in the database and updates additional fields.
        /// </summary>
        /// <remarks>This method performs the following operations within a database transaction: <list
        /// type="bullet"> <item><description>Adds the specified member to the database.</description></item>
        /// <item><description>Updates additional fields for the member, including special handling for the "DOB"
        /// field.</description></item> <item><description>Increments the user count in the totals
        /// table.</description></item> </list> If an error occurs during the operation, the transaction is rolled back,
        /// and the exception is rethrown.</remarks>
        /// <param name="member">The <see cref="Member"/> object to be added to the database. This object must not be null.</param>
        /// <param name="additionalFields">A list of key-value pairs representing additional fields to update for the member.  The key specifies the
        /// field name, and the value specifies the field value.  Special handling is applied for the "DOB" field, which
        /// must be in the format "dd/mm/yyyy".</param>
        /// <returns>The created <see cref="Member"/> object with its database-generated values populated.</returns>
        public Member Create(Member member,List<KeyValuePair<string,object>> additionalFields)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var result = _dbContext.Members.Add(member);
                _dbContext.SaveChanges();
                foreach (var additionalField in additionalFields)
                {
                    if (additionalField.Key.ToUpper() == "DOB")
                    {
                        var dob = DateTime.ParseExact(additionalField.Value.ToString()!,"dd/mm/yyyy",CultureInfo.InvariantCulture).ToString("yyyyMMdd");
                        var memberid = member.Id;
                        _dbContext.Database.ExecuteSqlRaw("UPDATE " + _memberprefix + "MEMBERS SET M_"+additionalField.Key.ToUpper()+$"={dob} WHERE MEMBER_ID={memberid}");
                    }
                    else
                    {
                        string? country = additionalField.Value?.ToString();
                        var sql = "UPDATE " + _memberprefix + "MEMBERS SET M_"+additionalField.Key.ToUpper();
                        var memberid = member.Id;
                        _dbContext.Database.ExecuteSqlRaw(sql + "={0} WHERE MEMBER_ID={1} ",country,memberid);
                    }
                }

                _dbContext.Database.ExecuteSqlRaw($"UPDATE "+_tableprefix+"TOTALS SET U_COUNT = U_COUNT + 1;");
                transaction.Commit();

                return result.Entity;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }

        }

        /// <summary>
        /// Retrieves the current rankings of members.
        /// </summary>
        /// <remarks>The rankings are cached for 10 minutes to improve performance. Subsequent calls
        /// within this period will return the cached data.</remarks>
        /// <returns>A dictionary where the key is the member ID and the value is the corresponding <see cref="MemberRanking"/>
        /// object.  Returns <see langword="null"/> if no rankings are available.</returns>
        public Dictionary<int, MemberRanking>? GetRankings()
        {
            return CacheProvider.GetOrCreate("Snitz.Rankings", () => Rankings(), TimeSpan.FromMinutes(10));

        }

        /// <summary>
        /// Retrieves a dictionary of member rankings, keyed by their unique identifiers.
        /// </summary>
        /// <remarks>This method queries the database context for member rankings and returns a dictionary
        /// containing the rankings. If duplicate keys are encountered, only the first occurrence is added to the
        /// dictionary. Any exceptions encountered during execution are suppressed, and the method will return an empty
        /// dictionary in such cases.</remarks>
        /// <returns>A dictionary where the key is the unique identifier of the member ranking, and the value is the
        /// corresponding <see cref="MemberRanking"/> object. Returns an empty dictionary if an error occurs or no
        /// rankings are found.</returns>
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
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
                //Supress any errors
            }
            return rankings;
        }

        /// <summary>
        /// Retrieves a member from the database based on the specified user's identity.
        /// </summary>
        /// <remarks>This method performs a case-sensitive lookup based on the user's identity name. The
        /// returned member is retrieved without tracking changes in the database context.</remarks>
        /// <param name="user">The <see cref="ClaimsPrincipal"/> representing the user whose associated member is to be retrieved. The
        /// user's identity must not be null.</param>
        /// <returns>The <see cref="Member"/> associated with the specified user, or <see langword="null"/> if no matching member
        /// is found.</returns>
        public Member? GetMember(ClaimsPrincipal user)
        {
            return _dbContext.Members.AsNoTracking().SingleOrDefault(m=>m.Name == user.Identity!.Name);
        }

        /// <summary>
        /// Retrieves a collection of member name filters.
        /// </summary>
        /// <remarks>This method returns all member name filters from the underlying data source. The
        /// caller can enumerate the collection to access individual filters.</remarks>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="MemberNamefilter"/> representing the available member name
        /// filters.</returns>
        public IEnumerable<MemberNamefilter> UserNameFilter()
        {
            return _dbContext.MemberNamefilter;
        }

        /// <summary>
        /// Updates the last activity timestamp and related information for the currently authenticated user.
        /// </summary>
        /// <remarks>This method updates the user's last activity timestamp and, if applicable, their last
        /// login timestamp. If the user has been inactive for more than 20 minutes, the last login timestamp is reset
        /// to the last recorded activity timestamp. Additionally, the user's last IP address is updated based on the
        /// current HTTP context. Changes are persisted to the database.</remarks>
        /// <param name="user">The <see cref="ClaimsPrincipal"/> representing the currently authenticated user. The user must be
        /// authenticated and have a valid identity. If <paramref name="user"/> is null, unauthenticated, or lacks a
        /// valid identity, the method performs no action.</param>
        public void SetLastHere(ClaimsPrincipal? user)
        {
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
            {
                return;
            }
            var trackedEntity = _dbContext.ChangeTracker.Entries<Member>()
                .FirstOrDefault(e => e.Entity.Name == user.Identity.Name && e.Entity.Status == 1);
            if (trackedEntity != null)
                trackedEntity.State = EntityState.Detached;

            var member = _dbContext.Members.AsNoTracking().SingleOrDefault(m=>m.Name == user.Identity.Name && m.Status == 1);
            if (member == null)
            {
                return;
            }
                var now = DateTime.UtcNow.ToForumDateStr();
            //if there has been no activity for 10 minutes, reset the last login (LastLogin) date
            if (member.Lastactivity.FromForumDateStr().AddMinutes(20) < DateTime.UtcNow)
            {
                var lastactivity = member.Lastactivity;
                var lastip = _contextAccessor.HttpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString();
                if (lastactivity != null) _cookie.SetLastVisitCookie(lastactivity);
                _dbContext.Members.Where(m=>m.Name == user.Identity.Name && m.Status == 1)
                    .ExecuteUpdateAsync(s => s
                    .SetProperty(b => b.LastLogin, lastactivity)
                    .SetProperty(b => b.LastIp, lastip)
                    .SetProperty(b => b.Lastactivity, now)
                    );
                return;
            }            
            _dbContext.Members.Where(m=>m.Name == user.Identity.Name && m.Status == 1)
                .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.Lastactivity, now));

        }

        /// <summary>
        /// Deletes the specified member from the database.
        /// </summary>
        /// <remarks>This method removes the specified member from the database and commits the changes
        /// immediately. Ensure that the provided member exists in the database before calling this method.</remarks>
        /// <param name="newmember">The member to be deleted. This parameter cannot be <see langword="null"/>.</param>
        public void Delete(Member newmember)
        {
            _dbContext.Members.Remove(newmember);
            _dbContext.SaveChanges();
        }

        /// <summary>
        /// Retrieves all members from the database, optionally filtering by active status.
        /// </summary>
        /// <param name="isadmin">A value indicating whether the caller has administrative privileges.  If <see langword="true"/>, all members
        /// are returned; otherwise, only members with an active status are included.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Member"/> objects representing the members retrieved. If
        /// <paramref name="isadmin"/> is <see langword="false"/>, only members with an active status are included.</returns>
        public IEnumerable<Member> GetAll(bool isadmin)
        {
            return isadmin ? _dbContext.Members : _dbContext.Members.Where(m => m.Status == 1);
        }

        /// <summary>
        /// Retrieves a paginated list of members, optionally filtered and sorted based on the specified criteria.
        /// </summary>
        /// <remarks>This method supports sorting by various columns and allows filtering based on the
        /// caller's administrative privileges. If the caller is not an administrator, only active members are included
        /// in the results.</remarks>
        /// <param name="isadmin">A value indicating whether the caller has administrative privileges. If <see langword="true"/>, all members
        /// are included; otherwise, only active members (with a status of 1) are included.</param>
        /// <param name="pagesize">The number of members to include per page. The default value is 20.</param>
        /// <param name="page">The page number to retrieve. The default value is 1.</param>
        /// <param name="sortcol">The column by which to sort the results. Supported values include "name", "lastpost", "lastvisit",
        /// "membersince", and "posts". If <see langword="null"/>, the results are sorted by "posts" in descending
        /// order.</param>
        /// <param name="dir">The sort direction. Supported values are "asc" for ascending and "desc" for descending. The default value is
        /// "asc".</param>
        /// <returns>A paginated list of members as an <see cref="IPagedList{T}"/>. The list contains members filtered and sorted
        /// based on the specified parameters. If no members match the criteria, an empty list is returned.</returns>
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

        /// <summary>
        /// Retrieves the name of a member based on the specified identifier.
        /// </summary>
        /// <remarks>The method searches for a member with the specified identifier and returns the name
        /// of the first match. If no member with the given identifier exists, the method returns <see
        /// langword="null"/>.</remarks>
        /// <param name="id">The unique identifier of the member to retrieve.</param>
        /// <returns>The name of the member if found; otherwise, <see langword="null"/>.</returns>
        public string? GetMemberName(int id)
        {
            return _dbContext.Members
                .OrderBy(m=>m.Id)
                .FirstOrDefault(m => m.Id == id)?.Name;
        }

        /// <summary>
        /// Retrieves all members with the specified email address.
        /// </summary>
        /// <param name="email">The email address to search for. This value cannot be null.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Member"/> objects that have the specified email address. If no
        /// members are found, the returned collection will be empty.</returns>
        public IEnumerable<Member> GetByEmail(string email)
        {
            return _dbContext.Members
                .Where(m => m.Email == email);
        }

        /// <summary>
        /// Retrieves a member by their username, including their active subscriptions.
        /// </summary>
        /// <remarks>The method performs a case-sensitive search for an active member with the specified
        /// username. The returned member includes their associated subscriptions, but the data is retrieved in a 
        /// read-only (no-tracking) state.</remarks>
        /// <param name="username">The username of the member to retrieve. This value cannot be null or empty.</param>
        /// <returns>The <see cref="Member"/> object that matches the specified username and is active,  or <see
        /// langword="null"/> if no such member exists.</returns>
        public Member? GetByUsername(string username)
        {
            return _dbContext.Members.AsNoTracking()
                .Include(m=>m.Subscriptions).AsNoTracking()
                //in case multiple
                .FirstOrDefault(m=>m.Name == username && m.Status == 1);
        }

        /// <summary>
        /// Retrieves a paginated list of members whose names start with the specified initial character, optionally
        /// filtered by administrative status and sorted by the specified column and direction.
        /// </summary>
        /// <remarks>This method filters members based on their name's initial character and optionally
        /// their status. Sorting is applied based on the specified column and direction. If no sorting column or
        /// direction is provided, the default order is used. The method also calculates the total number of matching
        /// members and returns it via the <paramref name="totalcount"/> parameter.</remarks>
        /// <param name="isadmin">A value indicating whether to include all members (<see langword="true"/>) or only active members (<see
        /// langword="false"/>).</param>
        /// <param name="initial">The initial character to filter member names. The search is case-insensitive.</param>
        /// <param name="totalcount">When the method returns, contains the total number of members matching the specified criteria.</param>
        /// <param name="pagesize">The number of members to include in each page of the result.</param>
        /// <param name="page">The page number to retrieve, where the first page is 1.</param>
        /// <param name="sortcol">The name of the column to sort the results by. Valid values are "name", "lastpost", "lastvisit",
        /// "membersince", and "posts". If <see langword="null"/>, no specific sorting is applied.</param>
        /// <param name="dir">The direction of sorting. Valid values are "asc" for ascending order and "desc" for descending order. If
        /// <see langword="null"/>, the default sorting direction is applied.</param>
        /// <returns>A paginated list of members matching the specified criteria. The list may be empty if no members match the
        /// criteria.</returns>
        public IPagedList<Member> GetByInitial(bool isadmin, string initial,out int totalcount,int pagesize, int page,string? sortcol,string? dir)
        {
            var members = _dbContext.Members.Where(m=>m.Name.ToLower().StartsWith(initial.ToLower())).ToList();
            if (!isadmin)
            {
                members = members.Where(m=>m.Status == 1).ToList();
            }
            totalcount = members.Count();

            if (sortcol != null)
            {
                switch (sortcol)
                {
                    case "name" :
                        switch (dir)
                        {
                            case "asc" :
                                return members.OrderBy(p => p.Name).ToPagedList(page, pagesize);
                            default :
                                return members.OrderByDescending(p => p.Name).ToPagedList(page, pagesize);
                        }
                    case "lastpost" :
                        switch (dir)
                        {
                            case "asc" :
                                return members.OrderBy(p => p.Lastpostdate).ToPagedList(page, pagesize);
                            default :
                                return members.OrderByDescending(p => p.Lastpostdate).ToPagedList(page, pagesize);
                        }
                    case "lastvisit" :
                        switch (dir)
                        {
                            case "asc" :
                                return members.OrderBy(p => p.LastLogin).ToPagedList(page, pagesize);
                            default :
                                return members.OrderByDescending(p => p.LastLogin).ToPagedList(page, pagesize);
                        }
                    case "membersince" :
                        switch (dir)
                        {
                            case "asc" :
                                return members.OrderBy(p => p.Created).ToPagedList(page, pagesize);
                            default :
                                return members.OrderByDescending(p => p.Created).ToPagedList(page, pagesize);
                        }
                    case "posts" :
                        switch (dir)
                        {
                            case "asc" :
                                return members.OrderBy(p => p.Posts).ToPagedList(page, pagesize);
                            default :
                                return members.OrderByDescending(p => p.Posts).ToPagedList(page, pagesize);
                        }
                }
            }
            
            return members.ToPagedList(page, pagesize);
        }

        /// <summary>
        /// Retrieves a filtered collection of members based on the specified search query and search field.
        /// </summary>
        /// <remarks>The search is case-insensitive and will return all members whose specified field
        /// contains the  <paramref name="searchQuery"/> as a substring. If the <paramref name="searchField"/> is
        /// invalid,  the method returns <see langword="null"/>.</remarks>
        /// <param name="searchQuery">The search term to filter members by. This value is case-insensitive.</param>
        /// <param name="searchField">The field to search within. Valid values are: <list type="bullet"> <item><description>"1" - Searches by
        /// member name.</description></item> <item><description>"2" - Searches by member first
        /// name.</description></item> <item><description>"3" - Searches by member last name.</description></item>
        /// <item><description>"4" - Searches by member rank title.</description></item> <item><description>"5" -
        /// Searches by member email address.</description></item> </list></param>
        /// <returns>A collection of <see cref="Member"/> objects that match the search criteria, or <see langword="null"/>  if
        /// the <paramref name="searchField"/> is invalid.</returns>
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

        /// <summary>
        /// Updates the specified member in the database.
        /// </summary>
        /// <remarks>The method marks the entity as modified and saves the changes to the database. 
        /// Ensure that the provided <paramref name="member"/> object is properly initialized and represents an existing
        /// record.</remarks>
        /// <param name="member">The member entity to update. The entity must already exist in the database.</param>
        public void Update(Member member)
        {

            _dbContext.Attach(member).State = EntityState.Modified;
            //_dbContext.Update(member);
            _dbContext.SaveChanges(true);
        }

        /// <summary>
        /// Creates a new member with the specified name and email, and adds it to the database.
        /// </summary>
        /// <remarks>The new member is initialized with a default level of 1 and an active status. Ensure
        /// that the provided email address is unique within the database to avoid potential conflicts.</remarks>
        /// <param name="userName">The name of the user to create. Cannot be null or empty.</param>
        /// <param name="userEmail">The email address of the user to create. Must be a valid email format and cannot be null or empty.</param>
        /// <returns>The unique identifier of the newly created member.</returns>
        public int Create(string userName, string userEmail)
        {
            var member = new Member { Email = userEmail, Name = userName, Level = 1, Status = 1};
            var result = _dbContext.Members.Add(member);
            _dbContext.SaveChanges();
            return result.Entity.Id;
        }

        /// <summary>
        /// Determines the rank title of a member based on their status, name, and other attributes.
        /// </summary>
        /// <remarks>This method evaluates the member's status and name to determine if a special rank
        /// title should be applied.  If no special conditions are met, the member's existing title is
        /// returned.</remarks>
        /// <param name="member">The member whose rank title is to be determined. Must not be <see langword="null"/>.</param>
        /// <returns>A string representing the rank title of the member. Returns "Member Locked" if the member's status is 0  or
        /// their name is "n/a". Returns "Zapped Member" if the member's name is "zapped". Otherwise, returns the 
        /// member's current title.</returns>
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

        /// <summary>
        /// Verifies that the provided password matches the specified hashed password.
        /// </summary>
        /// <remarks>This method uses the PBKDF2 algorithm with a fixed iteration count and key length to
        /// verify the password. The hashed password must conform to the expected format, which includes a version byte,
        /// salt, and subkey. If the hashed password does not match the expected format, the method returns <see
        /// langword="false"/>.</remarks>
        /// <param name="hashedPassword">The hashed password to verify. This must be a Base64-encoded string that includes the salt and subkey.</param>
        /// <param name="password">The plain-text password to verify against the hashed password.</param>
        /// <returns><see langword="true"/> if the hashed password matches the provided plain-text password; otherwise, <see
        /// langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="hashedPassword"/> or <paramref name="password"/> is <see langword="null"/>.</exception>
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

        /// <summary>
        /// Compares two byte arrays for equality in a time-invariant manner.
        /// </summary>
        /// <remarks>This method performs a constant-time comparison to mitigate timing attacks, ensuring
        /// that the comparison time does not vary based on the contents of the arrays. However, it is the caller's
        /// responsibility to ensure that this method is used in a security-sensitive context where such protection is
        /// necessary.</remarks>
        /// <param name="a">The first byte array to compare. Can be <see langword="null"/>.</param>
        /// <param name="b">The second byte array to compare. Can be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if both byte arrays are equal in length and contain the same sequence of bytes;
        /// otherwise, <see langword="false"/>. Returns <see langword="true"/> if both arrays are <see
        /// langword="null"/>.</returns>
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

        /// <summary>
        /// Retrieves a collection of forum IDs that the current user is subscribed to.
        /// </summary>
        /// <remarks>If the current user is not authenticated or their ID is unavailable, an empty
        /// collection is returned. The returned collection contains distinct forum IDs, ordered in ascending
        /// order.</remarks>
        /// <returns>An <see cref="IEnumerable{T}"/> of integers representing the IDs of the forums the current user is
        /// subscribed to. If the user is not authenticated, an empty collection is returned.</returns>
        public IEnumerable<int> ForumSubscriptions()
        {
            var memberid = Current()?.Id;
            if (memberid.HasValue)
            {
                return _dbContext.MemberSubscriptions.AsNoTracking().Where(s => s.MemberId == memberid && s.PostId == 0).Select(s => s.ForumId).Distinct().OrderBy(o=>o);
            }

            return new List<int>();
        }
        public IEnumerable<int> TopicSubscriptions()
        {
            var memberid = Current()?.Id;
            if (memberid.HasValue)
            {
                return _dbContext.MemberSubscriptions.AsNoTracking().Where(s => s.MemberId == memberid && s.PostId != 0).Select(s => s.PostId).Distinct().OrderBy(o=>o);
            }

            return new List<int>();
        }

        /// <summary>
        /// Retrieves a collection of recent members based on their last activity date.
        /// </summary>
        /// <remarks>A member is considered "recent" if their last activity occurred within the past 13
        /// months.</remarks>
        /// <param name="max">The maximum number of recent members to return. Must be a positive integer.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="RecentMembers"/> objects representing the most recently active
        /// members,  ordered by their last activity date in descending order. The collection will contain at most
        /// <paramref name="max"/> items.</returns>
        public IEnumerable<RecentMembers?> GetRecent(int max)
        {
            var recentcutoff = DateTime.UtcNow.AddMonths(-13).ToForumDateStr();   
            return _dbContext.Members.Select(m=>new RecentMembers(){Id = m.Id, Name = m.Name,Avatar = m.PhotoUrl, LastActivity = m.Lastactivity}).OrderByDescending(m=>m.LastActivity).Where(m=>string.Compare(m.LastActivity, recentcutoff) > 0).Take(max);
        }

        /// <summary>
        /// Updates the last post information for the specified member.
        /// </summary>
        /// <remarks>This method updates the <c>Lastpostdate</c>, <c>Lastactivity</c>, and <c>LastIp</c>
        /// fields for the specified member. If the member does not exist, the method performs no action.</remarks>
        /// <param name="memberid">The unique identifier of the member whose last post information is to be updated.</param>
        /// <returns></returns>
        public async Task UpdateLastPost(int memberid)
        {
            var member = _dbContext.Members.Find(memberid);
            if (member != null)
            {
                member.Lastpostdate = DateTime.UtcNow.ToForumDateStr();
                member.Lastactivity = DateTime.UtcNow.ToForumDateStr();
                member.LastIp = _contextAccessor.HttpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString();
                _dbContext.Members.Update(member);
                _dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// Deactivates a member account and updates the associated user information.
        /// </summary>
        /// <remarks>This method performs the following actions: <list type="bullet"> <item>Finds the
        /// member with the specified <paramref name="memberid"/> in the database.</item> <item>Updates the associated
        /// user account, including setting the email to a placeholder value, enabling lockout, and updating the
        /// security stamp.</item> <item>Creates a new "zapped" member record with default values and updates the
        /// database.</item> </list> If the specified member does not exist, the method returns <see langword="false"/>
        /// without making any changes.</remarks>
        /// <param name="memberid">The unique identifier of the member to be deactivated.</param>
        /// <returns><see langword="true"/> if the member was successfully deactivated; otherwise, <see langword="false"/>.</returns>
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
                try
                {
                    Member zappedMember = new()
                    {
                        Id = memberid,
                        Name = "zapped",
                        Email = "zapped@dummy.com",
                        Posts = member.Posts,
                        Status = 0,
                        Title = "Zapped Member",
                        Created = member.Created,
                        Level = 1, 
                        Ip = "0.0.0.0",
                        Sha256 = 1,
                    };
                    _dbContext.Update(zappedMember);
                    _dbContext.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.Message);
                    throw;
                }

            }
            return false;
        }

        /// <summary>
        /// Determines whether the specified member has rated the given topic.
        /// </summary>
        /// <param name="topicid">The unique identifier of the topic to check.</param>
        /// <param name="memberid">The unique identifier of the member. If <see langword="null"/>, the method assumes the topic has been rated.</param>
        /// <returns><see langword="true"/> if the specified member has rated the topic, or if <paramref name="memberid"/> is
        /// <see langword="null"/>; otherwise, <see langword="false"/>.</returns>
        public bool HasRatedTopic(int topicid, int? memberid)
        {
            if(memberid == null)
            {
                return true;
            }
            return _dbContext.TopicRating.Where(t=>t.RatingsTopicId == topicid && t.RatingsBymemberId == memberid).Any();
        }

        /// <summary>
        /// Retrieves a list of forum IDs that the specified user is allowed to view.
        /// </summary>
        /// <remarks>This method evaluates the user's roles and authentication status to determine access
        /// to forums based on their authorization type. - Administrators have access to all forums. - Forums with
        /// specific authorization types, such as <see cref="ForumAuthType.AllowedMembers"/> or <see
        /// cref="ForumAuthType.PasswordProtected"/>,    require the user to meet certain conditions (e.g., being in a
        /// specific role or being authenticated).</remarks>
        /// <param name="user">The user whose permissions are evaluated to determine viewable forums.</param>
        /// <returns>A list of forum IDs that the user has permission to access. The list will be empty if the user has no access
        /// to any forums.</returns>
        public List<int> ViewableForums(IPrincipal user)
        {
            List<int> allowed = new();
            foreach (var forum in _dbContext.Forums)
            {
                if (user.IsAdministrator())
                {
                    allowed.Add(forum.Id);
                }
                else
                {
                    switch (forum.Privateforums)
                    {
                        case ForumAuthType.All:
                            allowed.Add(forum.Id);
                            break;
                        case ForumAuthType.AllowedMembers:
                        case ForumAuthType.AllowedMemberPassword:
                            if (user.IsInRole("Forum_" + forum.Id) || user.IsInRole("FORUM_" + forum.Id))
                            {
                                allowed.Add(forum.Id);
                            }
                            break;
                        case ForumAuthType.MembersHidden:
                        case ForumAuthType.AllowedMembersHidden:

                            break;
                        case ForumAuthType.Members:
                        case ForumAuthType.MembersPassword:
                        case ForumAuthType.PasswordProtected:
                            if (user.Identity.IsAuthenticated)
                            {
                                allowed.Add(forum.Id);
                            }
                            break;
                    }
                }

            }
            return allowed;
        }

        /// <summary>
        /// Retrieves a collection of forum IDs that the current user is allowed to access.
        /// </summary>
        /// <remarks>This method determines the forums accessible to the current user based on their
        /// permissions and the forum's privacy settings. If the current HTTP context or user is unavailable, an empty
        /// collection is returned.</remarks>
        /// <returns>An <see cref="IEnumerable{T}"/> of integers representing the IDs of the forums the user is allowed to
        /// access. The collection will be empty if the user has no access to any forums or if the HTTP context is
        /// unavailable.</returns>
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
        
        /// <summary>
        /// Determines whether the specified user is allowed to access the forum based on the provided authorization
        /// type.
        /// </summary>
        /// <remarks>This method evaluates access permissions based on the user's role, the forum's
        /// authorization type, and the user's membership in the forum. - If <paramref name="user"/> is <see
        /// langword="null"/>, access is granted only if <paramref name="type"/> is <see cref="ForumAuthType.All"/>. -
        /// Users with the "Administrator" role are always granted access. - For authorization types that require
        /// specific forum membership, the method checks if the user is explicitly allowed in the forum.</remarks>
        /// <param name="forumid">The unique identifier of the forum to check access for.</param>
        /// <param name="user">The user whose access is being evaluated. Can be <see langword="null"/> to represent an unauthenticated
        /// user.</param>
        /// <param name="type">The type of authorization required to access the forum.</param>
        /// <returns><see langword="true"/> if the user is allowed to access the forum based on the specified authorization type;
        /// otherwise, <see langword="false"/>.</returns>
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
