using SnitzCore.Data.Models;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;

namespace SnitzCore.Data.Interfaces;

public interface ICategory
{
    Category? GetById(int id);
    IEnumerable<Category> GetAll();
    void Create(Category category);
    Task Delete(int categoryId);
    void Update(Category category);

    IEnumerable<Group> GetGroups();
    IEnumerable<GroupName> GetGroupNames();
    Task DeleteForums(int id);
    IEnumerable<Category> FetchCategoryForumList(IPrincipal user);
}