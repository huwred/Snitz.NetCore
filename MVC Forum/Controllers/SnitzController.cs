using System.IO;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Service;

namespace MVCForum.Controllers
{
    public class SnitzController : Controller
    {
        protected ISnitzConfig _config;
        protected IMember _memberService;
        protected LanguageService  _languageResource;
        protected SnitzDbContext _snitzDbContext;
        protected IHttpContextAccessor _httpContextAccessor;
        protected static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public SnitzController(IMember memberService, ISnitzConfig config,IHtmlLocalizerFactory localizerFactory,SnitzDbContext dbContext,IHttpContextAccessor httpContextAccessor)
        {
            _config = config;
            _memberService = memberService;
            _snitzDbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _languageResource = (LanguageService)localizerFactory.Create("SnitzController", "MVCForum");
        }


                /// <summary>
        /// Saves the contents of an uploaded image file.
        /// </summary>
        /// <param name="targetFolder">Location where to save the image file.</param>
        /// <param name="file">The uploaded image file.</param>
        /// <exception cref="InvalidOperationException">Invalid MIME content type.</exception>
        /// <exception cref="InvalidOperationException">Invalid file extension.</exception>
        /// <exception cref="InvalidOperationException">File size limit exceeded.</exception>
        /// <returns>The relative path where the file is stored.</returns>
        protected async Task<string> SaveFileAsync(string targetFolder, IFormFile file)
        {
            const int megabyte = 1024 * 1024;
            string[] extensions =  { ".gif", ".jpg", ".png", ".svg", ".webp" };
            //if (AllowedFiles.Any())
            //{
            //    extensions = AllowedFiles;
            //}

            if (!file.ContentType.StartsWith("image/"))
            {
                throw new InvalidOperationException(_languageResource["Forums.Error.MimeType"].Value);
            }
            var extension = Path.GetExtension(file.FileName.ToLowerInvariant());
            if (!extensions.Contains(extension))
            {
                throw new InvalidOperationException(_languageResource["Forums.Error.FileExt"].Value);
            }            
            //if (file.Length > (MaxFileSize * megabyte))
            //{
                
            //    throw new InvalidOperationException($"{_languageResource["Forums.Error.FileSize"].Value} ({MaxFileSize}MB)");
            //}            
            //var _fileSystem = _mediaFileManager.FileSystem;

            //if (!_fileSystem.DirectoryExists(_fileSystem.GetFullPath(targetFolder)))
            //{
            //    Directory.CreateDirectory(_fileSystem.GetFullPath(targetFolder));
            //}

            var fileName = file.FileName;
            //if (UniqueFilenames)
            //{
            //    fileName = Guid.NewGuid() + extension;
            //}
            //var path = Path.Combine(_fileSystem.GetFullPath(targetFolder), fileName);
            //await using (Stream fileStream = new FileStream(path, FileMode.Create)) {
            //    await file.CopyToAsync(fileStream);
            //}


            return Combine(targetFolder, fileName);
        }
        public static string Combine(string uri1, string uri2)
        {
            uri1 = uri1.TrimEnd('/');
            uri2 = uri2.TrimStart('/');
            return $"{uri1}/{uri2}";
        }

    }
}
