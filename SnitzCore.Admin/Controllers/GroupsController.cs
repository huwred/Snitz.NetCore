using Microsoft.AspNetCore.Mvc;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using SnitzCore.BackOffice.ViewModels;
using Microsoft.AspNetCore.Http;
using X.PagedList.Extensions;

namespace SnitzCore.BackOffice.Controllers
{
    public class GroupsController : Controller
    {
        private readonly ICategory _categoryservice;
        private readonly IGroups _groupservice;


        public GroupsController(ICategory category,IGroups groups)
        {

            _categoryservice = category;
            _groupservice = groups;

        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ManageGroups(int id = 0)
        {
            AdminGroupsViewModel vm = new AdminGroupsViewModel(id,_categoryservice)
            {
                GroupName = _categoryservice.GetGroupNames().FirstOrDefault(g=>g.Id == id)?.Name,
                Groups = _categoryservice.GetGroupNames().ToList(),
            };

            return View(vm);
        }
        public IActionResult UpdateGroupName(int GroupNameId, string GroupName)
        {
            var group = _categoryservice.GetGroupNames().FirstOrDefault(g=>g.Id == GroupNameId);
            if(group != null)
            {
                group.Name = GroupName;
                _groupservice.Update(group);
            }

            return PartialView("SaveResult", "Name updated");
        }
        /// <summary>
        /// Adds a new group
        /// </summary>
        /// <param name="groupname">Name of the new group</param>
        /// <returns></returns>
        [HttpPost]
        
        public async Task<IActionResult> AddGroup(string groupname) { 
            var group = new GroupName();
            group.Name = groupname;
            group.Description = groupname;
            await _groupservice.Add(group);

            return PartialView("SaveResult", "Group Added");
        }
        /// <summary>
        /// Adds a Forum category to the group
        /// </summary>
        /// <param name="vm">requires a groupnameId and a categoryId</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddCatGroup(IFormCollection vm) { 
            var group = vm["GroupName"];
            var groupnameId = vm["GroupNameId"];
            var categories = vm["Categories"].Split(',');
            foreach (var category in categories) {
                Group newg = new Group
                {
                    GroupNameId = Convert.ToInt32(groupnameId),
                    CategoryId = Convert.ToInt32(category.First())
                };
                await _groupservice.Create(newg);
                
            }

            return PartialView("SaveResult", "Category added to " + group);
        }
        [HttpPost]
        public IActionResult RemCatGroup(IFormCollection vm) { 
            var group = vm["GroupName"];
            var groupnameId = vm["GroupNameId"];
            var categories = vm["Categories"].Split(',');
            foreach (var category in categories) {
                
                _groupservice.DeleteCategory(Convert.ToInt32(groupnameId),Convert.ToInt32(category.First()));
                
            }
            return PartialView("SaveResult", group + " Updated");
        }
    }
}
