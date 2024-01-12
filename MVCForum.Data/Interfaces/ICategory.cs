using SnitzCore.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SnitzCore.Data.Interfaces;

public interface ICategory
{
    Category GetById(int id);
    IEnumerable<Category> GetAll();
    Task Create(Category category);
    Task Delete(int categoryId);
    Task Update(Category category);

    IEnumerable<Group> GetGroups();
    IEnumerable<GroupName> GetGroupNames();
    Task DeleteForums(int id);
}