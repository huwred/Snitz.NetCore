using SnitzCore.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnitzCore.Data.Interfaces
{
    public interface IGroups
    {
        Task Add(GroupName group);

        Task Create(Group group);
        Task Update(Group group);
        Task Delete(GroupName group);
        Task DeleteCategory(int groupnameid, int categoryid);
        List<Group> GetAll();
        List<GroupName> GetNames();

        List<Group> GetGroups(int groupnameid);
    }
}
