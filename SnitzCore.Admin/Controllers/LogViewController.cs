using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SnitzCore.BackOffice.ViewModels;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace SnitzCore.BackOffice.Controllers
{
    [Authorize(Roles="Administrator")]
    public class LogViewController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<LogViewController> _logger;
        private string logFilePath = "";
        private IList<LogInfo>? _currentlog; 
        public LogViewController(IWebHostEnvironment env, ILogger<LogViewController> logger) { 
            _env = env;
            _logger = logger;

            logFilePath = Path.Combine(_env.ContentRootPath, "logs");


        }

        public IActionResult Index(string? id)
        {
            DirectoryInfo d = new DirectoryInfo(logFilePath);
            FileInfo[] Files = d.GetFiles("*.json"); // Get html files
            var vm = new LogViewModel();
            vm.Logs = Files.OrderByDescending(m=>m.Name).ToArray();

            if(id != null) {
                string jsonText = "";

                using (FileStream fs = new FileStream(Path.Combine(logFilePath,id), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader reader = new StreamReader(fs))
                {
                    jsonText = reader.ReadToEnd().Replace(Environment.NewLine,",");

                }
                _currentlog = JsonConvert.DeserializeObject<IList<LogInfo>>("[" +jsonText + "]");
                vm.CurrentLog = _currentlog;
                HttpContext.Session.SetString("logfile", id);
                TempData.Keep();
            }


            return View(vm);
        }
        public IActionResult DeleteLog(string file)
        {
            var filepath = Path.Combine(logFilePath,file);
            try
            {
                System.IO.File.Delete(filepath);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                return NoContent();
            }
            return Ok();
        }

        public JsonResult CustomServerSideSearchAction(DataTableAjaxPostModel model)
        {
            // action inside a standard controller
            int filteredResultsCount;
            int totalResultsCount;
            var result = YourCustomSearchFunc(model, out filteredResultsCount, out totalResultsCount);
 
            return Json(new
            {
                // this is what datatables wants sending back
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = result
            });
        }

        public IList<LogInfo> YourCustomSearchFunc(DataTableAjaxPostModel model, out int filteredResultsCount, out int totalResultsCount)
        {
            var searchBy = model.search?.value;
            var take = model.length;
            var skip = model.start;
 
            string sortBy = "";
            bool sortDir = true;
 
            if (model.order != null)
            {
                // in this example we just default sort on the 1st column
                sortBy = model.columns![model.order![0].column!].data!;
                sortDir = model.order![0].dir!.ToLower() == "asc";
            }
 
            // search the dbase taking into consideration table sorting and paging
            var result = GetDataFromFile(searchBy!, take, skip, sortBy, sortDir, out filteredResultsCount, out totalResultsCount);
            if (result == null)
            {
                // empty collection...
                return new List<LogInfo>();
            }
            return result;
        }
        public List<LogInfo> GetDataFromFile(string searchBy, int take, int skip, string sortBy, bool sortDir, out int filteredResultsCount, out int totalResultsCount)
        {
            var file = HttpContext.Session.GetString("logfile");
            if (file == null) {
                filteredResultsCount = 0;
                totalResultsCount = 0;
                return new List<LogInfo>();
            }
            string jsonText = "";
            using (FileStream fs = new FileStream(Path.Combine(logFilePath,file), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader reader = new StreamReader(fs))
                {
                    jsonText = reader.ReadToEnd().Replace(Environment.NewLine,",");

                }
                _currentlog = JsonConvert.DeserializeObject<IList<LogInfo>>("[" +jsonText + "]");
            if (String.IsNullOrEmpty(searchBy))
            {
                // if we have an empty search then just order the results by Id ascending
                sortBy = "date";
                sortDir = true;
            }
            if(_currentlog != null)
            {
                var result = _currentlog.AsEnumerable()
                               //.Where(whereClause)
                               .OrderByDescending(l=>l.date)
                               //.OrderBy(sortBy, sortDir) // have to give a default order when skipping .. so use the PK
                               .Skip(skip)
                               .Take(take)
                               .ToList();
 
                // now just get the count of items (without the skip and take) - eg how many could be returned with filtering
                //filteredResultsCount = _currentlog.Where(whereClause).Count();
                filteredResultsCount = _currentlog.Count();
                totalResultsCount = _currentlog.Count();
 
                return result;
            }

            filteredResultsCount = 0;
            totalResultsCount = 0;
            return new List<LogInfo>();
        }
    }
    public class LogViewModel
    {
        public FileInfo[]? Logs { get; set; }
        public IList<LogInfo>? CurrentLog { get; set; }
    }
    public class LogInfo
    {
        public DateTime date { get; set; }
        public string? level { get; set; }
        public string? logger { get; set; }
        public string? message { get; set; }

        public string? exception { get; set; }
    }
}
