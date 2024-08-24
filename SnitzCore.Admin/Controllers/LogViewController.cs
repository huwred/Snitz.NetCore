using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace SnitzCore.BackOffice.Controllers
{
    [Authorize(Roles="Administrator")]
    public class LogViewController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<LogViewController> _logger;
        private string logFilePath = "";
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
                vm.CurrentLog = JsonConvert.DeserializeObject<IList<LogInfo>>("[" +jsonText + "]");

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
    }
    public class LogViewModel
    {
        public FileInfo[] Logs { get; set; }
        public IList<LogInfo>? CurrentLog { get; set; }
    }
    public class LogInfo
    {
        public DateTime date { get; set; }
        public string level { get; set; }
        public string logger { get; set; }
        public string message { get; set; }

        public string? exception { get; set; }
    }
}
