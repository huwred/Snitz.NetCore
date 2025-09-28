using Microsoft.AspNetCore.Identity;
using SnitzCore.Data.Models;
using SnitzCore.Data.ViewModels;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using X.PagedList;

namespace SnitzCore.Data
{
    public interface IMember
    {
        Member? GetById(int? id);
        Member? Get(int id);
        Task<List<Member>> GetUsersInRoleAsync(string roleName);
        Task<Member?> GetById(ClaimsPrincipal user);
        IPagedList<Member?> GetPagedMembers(bool isadmin,int pagesize, int page,string? sortcol,string? dir);
        string? GetMemberName(int id);
        IEnumerable<Member?> GetByEmail(string email);
        Member? GetByUsername(string username);
        IPagedList<Member?> GetByInitial(bool isadmin,string initial,out int totalcount,int pagesize, int page,string? sortcol,string? dir);
        IEnumerable<Member?>? GetFilteredMembers(string searchQuery, string searchField);
        Member? GetMember(ClaimsPrincipal user);
        IEnumerable<MemberNamefilter> UserNameFilter();
        void Update(Member member);
        int Create(string userName, string userEmail);
        Member Create(Member member);
        Member Create(Member member, List<KeyValuePair<string, object>> additional);
        Dictionary<int, MemberRanking>? GetRankings();
        void SetLastHere(ClaimsPrincipal? user);
        void Delete(Member newmember);

        string SHA256Hash(string password);

        IList<string> Roles(string username);

        MigratePassword ValidateMember(Member member, string password);

        Member? Current();
        Task UpdatePostCount(int memberid);
        IEnumerable<int> ForumSubscriptions();
        IEnumerable<Member?> GetAll(bool isInRole);
        IEnumerable<RecentMembers?>  GetRecent(int max);
        Task UpdateLastPost(int memberid);
        bool ZapMember(int memberid);
        bool HasRatedTopic(int topicid,int? memberid);
        IEnumerable<int> AllowedForums();
        List<int> ViewableForums(IPrincipal user);
    }
}
