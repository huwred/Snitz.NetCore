using SnitzCore.Data.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using X.PagedList;

namespace SnitzCore.Data
{
    public interface IMember
    {
        Task<Member> GetById(int? id);
        Task<Member> GetById(ClaimsPrincipal user);
        IEnumerable<Member> GetAll();
        IPagedList<Member> GetPagedMembers(int pagesize, int page);
        string GetMemberName(int id);
        IEnumerable<Member> GetByEmail(string email);
        Member? GetByUsername(string username);
        IPagedList<Member> GetByInitial(string initial,out int totalcount,int pagesize = 20, int page = 1);
        IEnumerable<Member> GetFilteredMembers(string searchQuery, string searchField);
        Member GetMember(ClaimsPrincipal user);
        IEnumerable<MemberNamefilter> UserNameFilter();
        void Update(Member member);
        int Create(string userName, string userEmail);
        Member Create(Member member);
        Dictionary<int, MemberRanking> GetRankings();
        void SetLastHere(ClaimsPrincipal user);
        void Delete(Member newmember);

        string SHA256Hash(string password);

        IList<string> Roles(string username);

        bool ValidateMember(Member member, string password);
    }
}
