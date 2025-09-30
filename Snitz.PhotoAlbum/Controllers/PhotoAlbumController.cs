using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities.Zlib;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.Processing;
using SmartBreadcrumbs.Nodes;
using Snitz.PhotoAlbum.Models;
using Snitz.PhotoAlbum.ViewModels;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Service;
using SnitzCore.Service.Extensions;
using System.Dynamic;
using X.PagedList;
using static Dapper.SqlMapper;


namespace Snitz.PhotoAlbum.Controllers
{
    [CustomAuthorize]
    public class PhotoAlbumController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly PhotoContext _dbContext;
        private readonly IMember _memberservice;
        private readonly LanguageService  _languageResource;
        private readonly ISnitzConfig _config;
        private readonly IFileProvider _fileProvider;
        protected static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public PhotoAlbumController(IWebHostEnvironment hostingEnvironment, PhotoContext dbContext,IMember memberservice,IHtmlLocalizerFactory localizerFactory,ISnitzConfig config)
        {
            _environment = hostingEnvironment;
            _dbContext = dbContext;
            _memberservice = memberservice;
            _config = config;
            _fileProvider = hostingEnvironment.WebRootFileProvider;
            _languageResource = (LanguageService)localizerFactory.Create("SnitzController", "MVCForum");
        }

        public IActionResult Admin()
        {
            return View(new AdminAlbumViewModel());
        }
        public IActionResult Index(int page = 1, string sortdir = "asc", string orderby = "user", string term = "")
        {
            var albumPage = new MvcBreadcrumbNode("", "PhotoAlbum", _languageResource["mnuMemberAlbums"].Value);
            ViewData["BreadcrumbNode"] = albumPage;

            try
            {
                var model = _dbContext.AlbumImages
                    .Include(ai => ai.Member)
                    .Where(ai => ai.Member != null && ai.Member.Status == 1)
                    .GroupBy(p => new { p.MemberId,  p.Member.Name})
                    .Select(ai => new AlbumList
                    {
                        MemberId = ai.Key.MemberId,
                        Username = ai.Key.Name,
                        imgLastUpload = ai.Max(l=>l.Timestamp),
                        imgCount = ai.Count()
                    });

                ViewBag.SortUser = ViewBag.SortDate = ViewBag.SortCount = "asc";
                ViewBag.SortDir = sortdir;
                ViewBag.SortBy = orderby;
                ViewBag.SearchTerm = term;
                if (!string.IsNullOrWhiteSpace(term))
                {
                    model = model.Where(p => p.Username.Contains(term));
                }
                var totalCount = model.Count();
                var pageCount = 1;
                if (totalCount > 0)
                {
                    pageCount = (int)Math.Ceiling((double)totalCount! / 15);
                }
                ViewBag.PageCount = pageCount;
                ViewBag.Page = page;
                PagedList<AlbumList>? pagedImages = PagedPhotos(page, 15, sortdir,orderby, model);

                return View("Index",pagedImages);
            }
            catch (System.Exception ex)
            {
                ViewBag.Message = ex.Message;
                //This view is in main Snitz Views, just set a ViewBag.Message
                return View("Error");
            }
        }

        [HttpPost]
        public IActionResult Search(SearchViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.searchTerms))
            {
                return RedirectToAction("Album");
            }
            var albumPage = new MvcBreadcrumbNode("", "PhotoAlbum", _languageResource["mnuMemberAlbums"].Value);
            ViewData["BreadcrumbNode"] = albumPage;
            int pagesize = 24;

            try
            {
                var images = GetSpeciesEntries( vm.SortBy ?? "id", vm.SrchGroupId, true, vm.SrchIn.ToList(),vm.searchTerms,vm.SortOrder == "asc" ? "" : "1" );
                ViewBag.MemberId = 0;

                ViewBag.IsOwner = false;
                ViewBag.Display = 0;
                ViewBag.SortBy = vm.SortBy ?? "0";
                ViewBag.SortPage = vm.SortOrder;
                ViewBag.SortHead = vm.SortOrder == "asc" ? "desc" : "asc";
                ViewBag.SortDir = vm.SortOrder;
                //Paging info
                ViewBag.Page = 1;
                ViewBag.PageCount = (images.Count / pagesize) + 1;;

                var displayvm = new SpeciesAlbum
                {
                    GroupList = GetGroupList(),
                    Images = images,
                    Thumbs = true,
                    SpeciesOnly = true,
                    SortBy = vm.SortBy,
                    GroupFilter = vm.SrchGroupId
                };
                return View("Album", displayvm);
            }
            catch (Exception e)
            {
                ViewBag.Message = e.Message;
                //This view is in main Snitz Views, just set a ViewBag.Message
                return View("Error");
            }

            
        }
        public IActionResult GetPhoto(int? id)
        {
            var orgimage = _dbContext.Set<AlbumImage>().Find(id);
            var folder = StringExtensions.UrlCombine(_config.ContentFolder, "PhotoAlbum");
            if(orgimage != null)
            {
                return File(StringExtensions.UrlCombine(folder,$"{orgimage.Timestamp}_{orgimage.Location}"),"image/jpeg");
            }
            return File(StringExtensions.UrlCombine(_config.ContentFolder, "notfound.jpg") ,"image/jpeg");
        }

        /// <summary>
        /// Show a Members photo Album
        /// </summary>
        /// <param name="id"></param>
        /// <param name="display"> 0=thumbs,1=details,2=list,3=slideshow</param>
        /// <param name="pagenum"></param>
        /// <param name="sortby"></param>
        /// <param name="sortorder"></param>
        /// <returns></returns>
        public IActionResult Member(string id, int display = 0, int pagenum = 1, string sortby = "date",string sortorder = "desc",int category = 0)
        {
            try
            {
                Convert.ToInt32(id);
            }
            catch (Exception)
            {
                return View("Error");
            }
            var albumPage = new MvcBreadcrumbNode("", "PhotoAlbum", _languageResource["mnuMemberAlbums"].Value);

            int pagesize = 50;
            if (display == 2)
                pagesize = 30;
            if (display == 1)
                pagesize = 15;
            if (display == 3)
                pagesize = 50;

            var currentmemberid = _memberservice.Current()?.Id;

            var memberid = string.IsNullOrWhiteSpace(id) ? currentmemberid : Convert.ToInt32(id);

            if (memberid == null)
            {
                return View("Error");
            }

            var images = _dbContext.Set<AlbumImage>()
                .Include(i => i.Group)
                .Include(i=>i.Category)
                .Include(i => i.Member)
                .Where(i=>i.MemberId == memberid)
                .OrderByDescending(i=>i.Timestamp).ToList();

            ViewBag.Username = id;
            ViewBag.MemberId = memberid;

            if (memberid != null && memberid != currentmemberid)
            {
                var memberPage = new MvcBreadcrumbNode("Member", "PhotoAlbum",_languageResource["lblAlbum",_memberservice.GetMemberName(memberid.Value)].Value ) { Parent = albumPage};
                ViewData["BreadcrumbNode"] = memberPage;
            }
            else
            {
                var memberPage = new MvcBreadcrumbNode("Member", "PhotoAlbum", _languageResource["lblProfileLink"].Value) { Parent = albumPage};
                ViewData["BreadcrumbNode"] = memberPage;
            }

            if (currentmemberid != null) ViewBag.CurrentMemberId = currentmemberid;

            ViewBag.IsOwner = (memberid == currentmemberid);
            ViewBag.Display = display;
            TempData["Display"] = display;
            ViewBag.SortBy = sortby;
            ViewBag.SortPage = sortorder;
            ViewBag.SortHead = sortorder == "asc" ? "desc" : "asc";
            ViewBag.SortDir = sortorder;
            ViewData["Category"] = category;
            //Paging info
            ViewBag.Page = pagenum;
            ViewBag.PageCount = (images.Count() / pagesize) + 1;
            //SpeciesAlbum param = new()
            //{
            //    SortBy = sortby,
            //    SortDesc = sortorder == "asc" ? "" : "1",
            //    GroupFilter = 0,
            //    SrchUser = true,
            //    SrchTerm = id,
            //    SpeciesOnly = false
            //};
           // ViewBag.JsonParams = JsonConvert.SerializeObject(param);
            return View(images);

        }

        /// <summary>
        /// Show All images in Main album
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pagenum"></param>
        /// <param name="sortby"></param>
        /// <param name="groupFilter"></param>
        /// <param name="sortOrder"></param>
        /// <param name="showThumbs"></param>
        /// <param name="speciesOnly"></param>
        /// <returns></returns>
        public ActionResult Album(string id, int pagenum = 1, string sortby = "date", int groupFilter = -1,
            string sortOrder = "desc", bool showThumbs = true, int speciesOnly = 1)
        {
            var albumPage = new MvcBreadcrumbNode("Album", "PhotoAlbum", "Species Album");
            ViewData["BreadcrumbNode"] = albumPage;

            int pagesize = 24;

            int filter = 0;
            if (groupFilter > 0)
            {
                filter = groupFilter;
            }

            var images = GetSpeciesEntries( sortby, filter, speciesOnly == 1, null,null, sortOrder == "asc" ? "" : "1");
            ViewBag.Username = id;
            ViewBag.MemberId = 0;

            ViewBag.IsOwner = false;
            ViewBag.Display = 0;
            ViewBag.SortBy = sortby;
            ViewBag.SortPage = sortOrder;
            ViewBag.SortHead = sortOrder == "asc" ? "desc" : "asc";
            ViewBag.SortDir = sortOrder;// == "asc" ? "desc" : "asc";
            //Paging info
            ViewBag.Page = pagenum;
            ViewBag.PageCount = (images.Count / pagesize) + 1;;

            var vm = new SpeciesAlbum
            {
                GroupList = GetGroupList(),
                Images = images,
                Thumbs = showThumbs,
                SpeciesOnly = speciesOnly == 1,
                SortBy = sortby,
                GroupFilter = filter
            };

            //ViewBag.JsonParams = JsonConvert.SerializeObject(vm);
            return View(vm);

        }

        /// <summary>
        /// Displays the images in a Carousel
        /// </summary>
        /// <param name="id"></param>
        /// <param name="photoid"></param>
        /// <returns></returns>
        public ActionResult Gallery(int id = 0, int photoid = 0, string sortby = "date",string sortOrder = "desc", bool speciesOnly = true)
        {
            //todo:test member

            var images = GetSpeciesEntries(sortby, 0, speciesOnly, null,null, sortOrder == "desc" ? "1" : sortOrder == "1" ? "1" : "");

            var imageFiles = new ImageModel
            {
                CurrentIdx = photoid,
                CurrentImage = images[0]
            };

            ViewBag.Username = id;
            string rootFolder = "Content";

            string imagename =  $"{Url.Content($"{_config.RootFolder}/")}{rootFolder}/PhotoAlbum/";
                imageFiles.Images.AddRange(images.Select(f => new GalleryImage()
                {
                    Id = f.Id,
                    Name = f.ScientificName ?? f.CommonName ?? f.ImageName,
                    Path = imagename + f.ImageName,
                    Description = f?.Description
                }));


            ViewBag.footer = images[0].Member?.Name + " - " +
                             images[0].Timestamp.FromForumDateStr().ToLocalTime() + "<br/>" + images[0].Views +
                             " " + _languageResource.GetString("lblViews");
            ViewBag.title = string.Format("{0}, {1}, {2}", images[0].CommonName, images[0].ScientificName,
                images[0].Description);

            return View(imageFiles);

        }
        public IActionResult UploadForm(bool showall = false)
        {
            var model = new AlbumUploadViewModel
            {
                Group = -1, NotFeatured = false,
                GroupList = new SelectList(_dbContext.Set<AlbumGroup>().AsQueryable(), "Id", "Description"),
                AllowedTypes = _config.GetValue("STRIMAGETYPES"),
                MaxSize = _config.GetIntValue("INTMAXIMAGESIZE", 5)
            };

            foreach (var item in model.GroupList)
            {
                var lname = "Group_" + item.Text.Replace(" ", "");
                item.Text = _languageResource[lname].Value;
            }
            ViewBag.ShowAll = showall;
            return PartialView("_popAlbumUpload",model);
        }

        public IActionResult Thumbnail(int id)
        {
            var sep = Path.DirectorySeparatorChar;

            var uploadFolder = Path.Combine(_config.ContentFolder.Replace('\\',sep), "PhotoAlbum");
            var orgimage = _dbContext.Set<AlbumImage>().Find(id);
            if(orgimage == null)
            {
                return File("~/images/notfound.jpg", "image/jpeg");
            }
            try
            {

                var resizedPath = Path.Combine(uploadFolder,"thumbs");

                var fileInfo = _fileProvider.GetFileInfo(StringExtensions.UrlCombine(uploadFolder,$"{orgimage.Timestamp}_{orgimage.Location}"));
                if (!fileInfo.Exists)
                {
                    return File("~/images/notfound.jpg", "image/jpeg");
                }

                // Create the destination folder tree if it doesn't already exist
                if (!Directory.Exists(Path.Combine(_environment.WebRootPath, resizedPath)))
                {
                    resizedPath = Path.Combine(_environment.WebRootPath, resizedPath);
                    Directory.CreateDirectory(resizedPath);
                }
                var uploads = Path.Combine(_environment.WebRootPath, resizedPath);
                var filePath = Path.Combine(uploads, $"{orgimage.Timestamp}_{orgimage.Location}");

                var resizedInfo = _fileProvider.GetFileInfo(Path.Combine(resizedPath,$"{orgimage.Timestamp}_{orgimage.Location}"));
                if (!resizedInfo.Exists)
                {
                    using var inputStream = fileInfo.CreateReadStream();
                    using var image = Image.Load(inputStream);
                    using Image destRound = image.Clone(x => ConvertToThumb(x,new Size(_config.GetIntValue("INTMAXTHUMBSIZE",200), _config.GetIntValue("INTMAXTHUMBSIZE",200))));
                    destRound.SaveAsJpeg(filePath);
                }

                // resize the image and save it to the output stream

                return File(StringExtensions.UrlCombine(StringExtensions.UrlCombine(uploadFolder,"thumbs"),$"{orgimage.Timestamp}_{orgimage.Location}"), "image/jpeg");
            }
            catch (Exception e)
            {
                _logger.Error($"Thumbnail Error : {uploadFolder} {orgimage.ImageName}");
                return File("~/images/notfound.jpg", "image/jpeg");
            }

        }
        public JsonResult GetCaption(int id)
        {
            var photo = _dbContext.Set<AlbumImage>().Find(id);
            
            if(photo == null)
            {
                return Json("");
            }
            _dbContext.Entry<AlbumImage>(photo).State = EntityState.Detached;
            if (!String.IsNullOrWhiteSpace(photo?.Description))
            {
                return Json(photo.Description);
            }

            return Json("");
        }
        //[Route("PhotoAlbumUpload/")]
        public IActionResult Upload(AlbumUploadViewModel model)
        {
            var uploadFolder = StringExtensions.UrlCombine(_config.ContentFolder, "PhotoAlbum");

            var currentMember = _memberservice.Current();
            //var path = $"{uploadFolder}".Replace("/","\\");

            if (ModelState.IsValid)
            {
                var uniqueFileName = GetUniqueFileName(model.AlbumImage.FileName, out string timestamp);
                var uploads = Path.Combine(_environment.WebRootPath, _config.ContentFolder, "PhotoAlbum");
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }
                var filePath = Path.Combine(uploads, uniqueFileName);
                model.AlbumImage.CopyTo(new FileStream(filePath, FileMode.Create));

                var newalbumImage = new AlbumImage()
                {
                    Description = model.Description,
                    IsPrivate = model.Private,
                    DoNotFeature = model.NotFeatured,
                    CommonName = model.CommonName ?? "",
                    ScientificName = model.ScientificName ?? "",
                    GroupId = model.Group ?? 0,
                    MemberId = currentMember.Id,
                    Timestamp = timestamp,
                    Location = GetSafeFilename(model.AlbumImage.FileName),
                    CategoryId = model.Category
                };
                try
                {
                    _dbContext.Set<AlbumImage>().Add(newalbumImage);
                    _dbContext.SaveChanges();
                    model.GroupList = new SelectList(_dbContext.Set<AlbumGroup>().AsQueryable(), "Id", "Description");
                    var caption = model.ShowCaption ? model.Description : null;
                    return Json(new { result = true,id = newalbumImage.Id, data = StringExtensions.UrlCombine(uploadFolder,uniqueFileName),caption = caption });

                }
                catch (Exception e)
                {
                    return Json(new { result = false, data =  e.Message});
                }

            }

            return PartialView("_popAlbumUpload",model);

        }
        [HttpGet]
        public IActionResult MemberImages(int id, int display, int pagenum = 1, string sortby = "date",string sortorder = "desc", int category = 0)
        {
            int memberid = id;
            var currentmemberid = _memberservice.Current()?.Id;
            if (currentmemberid != null) ViewBag.CurrentMemberId = currentmemberid;

            var images = _dbContext.Set<AlbumImage>()
                .Include(i => i.Group)
                .Include(i=>i.Category)
                .Include(i => i.Member)
                .Where(i=>i.MemberId == memberid);
            //.OrderByDescending(i=>i.Timestamp);
            if (category > 0)
            {
                images = images.Where(i=>i.CategoryId == category);
            }
            if(sortorder == "asc")
            {
                switch (sortby)
                {
                    case "desc":
                        images = images.OrderBy(i => i.Description);
                        break;
                    case "date":
                        images = images.OrderBy(i => i.Timestamp);
                        break;
                    case "id":
                        images = images.OrderBy(i => i.Id);
                        break;
                    case "file":
                        images = images.OrderBy(i => i.Location);
                        break;
                }
            }
            else
            {
                switch (sortby)
                {
                    case "desc":
                        images = images.OrderByDescending(i => i.Description);
                        break;
                    case "date":
                        images = images.OrderByDescending(i => i.Timestamp);
                        break;
                    case "id":
                        images = images.OrderByDescending(i => i.Id);
                        break;
                    case "file":
                        images = images.OrderByDescending(i => i.Location);
                        break;
                }
            }

            ViewBag.Username = id;
            ViewBag.MemberId = memberid;
            ViewBag.IsOwner = (memberid == _memberservice.Current()?.Id);
            ViewBag.Display = display;
            TempData["Display"] = display;
            ViewBag.SortBy = sortby;
            ViewBag.SortPage = sortorder;
            ViewBag.SortHead = sortorder == "asc" ? "desc" : "asc";
            ViewBag.SortDir = sortorder;
            ViewData["Category"] = category;
            //Paging info
            ViewBag.Page = pagenum;
            //ViewBag.PageCount = db.Pagecount;

            return PartialView(images.ToList());
        }

        public IActionResult TogglePrivacy(int id, int memberid, bool state, int display)
        {
            var image = _dbContext.Set<AlbumImage>().Find(id);
            if (image != null)
            {
                image.IsPrivate = state;
                _dbContext.Update(image);
                _dbContext.SaveChanges();
            }

            return RedirectToAction("MemberImages", new {id=memberid,display });
        }
        public IActionResult ToggleDoNotFeature(int id, int memberid, bool state, int display)
        {
            var image = _dbContext.Set<AlbumImage>().Find(id);
            if (image != null)
            {
                image.DoNotFeature = state;
                _dbContext.Update(image);
                _dbContext.SaveChanges();
            }

            return RedirectToAction("MemberImages", new {id=memberid,display });
        }
        public IActionResult DeleteImage(int id, int memberid, int display)
        {
            var image = _dbContext.Set<AlbumImage>().Find(id);
            if (image != null)
            {
                _dbContext.Remove<AlbumImage>(image);
                _dbContext.SaveChanges();
            }

            return RedirectToAction("MemberImages", new {id=memberid,display });
        }
        public IActionResult Delete(AlbumImage img, int display=1)
        {
            var image = _dbContext.Set<AlbumImage>().Find(img.Id);
            if (image != null)
            {
                _dbContext.Remove<AlbumImage>(image);
                _dbContext.SaveChanges();
            }

            return RedirectToAction("Member", new {id=img.MemberId,display });
        }
        [HttpGet]
        [Authorize]
        public IActionResult Edit(int? id, int display=0)
        {
            if(id == null)
            {
                return View("Error");
            }
            var meta = MetaData(id.Value);

            var origimage = _dbContext.Set<AlbumImage>().AsNoTracking().Include(a=>a.Member).SingleOrDefault(a=>a.Id==id);
            if(meta != null && origimage != null)
            {
                ViewBag.ImageMeta = meta;
                ViewBag.MemberName = origimage.Member?.Name;
            }
                var model = new AlbumUploadViewModel()
                {
                    Description = origimage.Description,
                    Image = origimage,
                    GroupList = new SelectList(_dbContext.Set<AlbumGroup>().AsQueryable(), "Id", "Description",origimage.GroupId),
                    Display = display,
                    Category = origimage.CategoryId
                    
                };

            ViewBag.Display = display;
            return PartialView(model);
        }
        [HttpPost]
        public IActionResult Edit(AlbumUploadViewModel img)
        {
            var image = _dbContext.Set<AlbumImage>().Find(img.Image.Id);
            if (image != null)
            {
                image.CommonName = img.Image.CommonName;
                image.ScientificName = img.Image.ScientificName;
                image.Description = img.Image.Description;
                image.GroupId = img.Image.GroupId;
                image.CategoryId = img.Image.CategoryId;
                image.IsPrivate = img.Image.IsPrivate;
                image.DoNotFeature = img.Image.DoNotFeature;
                image.Width = img.Image.Width;
                image.Height = img.Image.Height;
                image.Size = img.Image.Size;
                image.Mime = img.Image.Mime;
                image.CategoryId = img.Category;
                _dbContext.Update(image);
                _dbContext.SaveChanges();
            }
            return RedirectToAction("MemberImages", new {id=img.Image.MemberId,display = img.Display });

            //return RedirectToAction("Member", new {id=img.Image.MemberId, display = img.Display });
        }
        public IActionResult UpdateGroup(AlbumGroup group)
        {
            var albumGroup = _dbContext.Set<AlbumGroup>().Find(group.Id);
            if (albumGroup != null)
            {
                albumGroup.Description = group.Description;
                albumGroup.Order = group.Order;
                _dbContext.Update(albumGroup);
                _dbContext.SaveChanges();
            }
            return Content("Updated");
        }

        [HttpPost]
        [Authorize]
        public IActionResult AddGroup(AlbumGroup group)
        {
            _dbContext.Set<AlbumGroup>().Add(group);
            _dbContext.SaveChanges();

            return Content("Group added, Reload page to view/edit groups.");
        }
        [HttpGet]
        [Authorize]
        public IActionResult DeleteGroup(int id)
        {
            _dbContext.Set<AlbumGroup>().Where(g => g.Id == id).ExecuteDelete();

            return Content("Group removed.");
        }

        private static PagedList<AlbumList>? PagedPhotos(int pagenum, int pagesize, string sortOrder, string sortBy, IQueryable<AlbumList> model)
        {
            if(!model.Any()) return null;
            PagedList<AlbumList> pagedReplies;
            switch (sortBy)
            {
                case "user":
                    pagedReplies = sortOrder == "asc"
                        ? new PagedList<AlbumList>(model.OrderBy(r => r.Username), pagenum, pagesize)
                        : new PagedList<AlbumList>(model.OrderByDescending(r => r.Username), pagenum, pagesize);
                    return pagedReplies;                    break;
                case "date":
                    pagedReplies = sortOrder == "asc"
                        ? new PagedList<AlbumList>(model.OrderBy(r => r.imgLastUpload), pagenum, pagesize)
                        : new PagedList<AlbumList>(model.OrderByDescending(r => r.imgLastUpload), pagenum, pagesize);
                    return pagedReplies;
                    break;
                case "count":
                    pagedReplies = sortOrder == "asc"
                        ? new PagedList<AlbumList>(model.OrderBy(r => r.imgCount), pagenum, pagesize)
                        : new PagedList<AlbumList>(model.OrderByDescending(r => r.imgCount), pagenum, pagesize);
                    return pagedReplies;                    break;
            }
            return null;
        }

        private IImageProcessingContext ConvertToThumb(IImageProcessingContext context, Size size)
        {
            var scaled = _config.GetValue("STRTHUMBTYPE") == "scaled";
            if (scaled)
            {
                size.Height = 0;
                return context.Resize(new ResizeOptions
                {
                    Size = size,
                    Mode = ResizeMode.Pad
                });
            }
            else
            {
                return context.Resize(new ResizeOptions
                {
                    Size = size,
                    Mode = ResizeMode.Crop
                });
            }

        }
        private List<AlbumImage> GetSpeciesEntries(string? sortby = "id", int groupid = 0, bool? speciesOnly = true, List<string>? searchin = null, string? searchfor = null, string sortDesc="")
        {
            var currentmemberid = _memberservice.Current()?.Id;

            var images = _dbContext.Set<AlbumImage>()
                .Include(i => i.Group)
                .Include(i=>i.Category)
                .Include(i => i.Member)
                .OrderByDescending(i=>i.Timestamp);

                if (searchin != null && searchin.Contains("Member"))
                {
                    if (_memberservice.GetByUsername(searchfor).Id != currentmemberid)
                    {
                        images = (IOrderedQueryable<AlbumImage>)images.Where(i=>!i.IsPrivate);
                    }
                }
                else if (searchin != null && searchin.Contains("MemberId"))
                {
                    if (Convert.ToInt32(searchfor) != currentmemberid)
                    {
                        images = (IOrderedQueryable<AlbumImage>)images.Where(i => !i.IsPrivate);
                    }
                }
                else
                {
                    images = (IOrderedQueryable<AlbumImage>)images.Where(i => !i.IsPrivate);
                }

                if (speciesOnly != null && speciesOnly.Value)
                {
                    images = (IOrderedQueryable<AlbumImage>)images.Where(i=> !string.IsNullOrWhiteSpace(i.ScientificName));
                    images = (IOrderedQueryable<AlbumImage>)images.Where(i=> !string.IsNullOrWhiteSpace(i.CommonName));
                    images = (IOrderedQueryable<AlbumImage>)images.Where(i=> !string.IsNullOrWhiteSpace(i.Description));
                }

            if (groupid > 0)
            {
                images = (IOrderedQueryable<AlbumImage>)images.Where(i => i.GroupId == groupid);
            }
            if (searchfor != null)
            {
                var predicate = PredicateBuilder.False<AlbumImage>();
                if (searchin != null)
                    foreach (string s in searchin)
                    {
                        switch (s)
                        {
                            case "Id":
                                predicate = predicate.Or(i=>i.Id.ToString().Contains(searchfor));
                                break;
                            case "Desc":
                                predicate = predicate.Or(i => i.Description != null && i.Description.Contains(searchfor));
                                break;
                            case "CommonName":
                                predicate = predicate.Or(i => i.CommonName != null && i.CommonName.Contains(searchfor));
                                break;
                            case "ScientificName":
                                predicate = predicate.Or(i => i.ScientificName != null && i.ScientificName.Contains(searchfor));
                                break;
                            case "Member":
                                predicate = predicate.Or(i => i.Member.Name.Contains(searchfor));
                                break;
                            case "MemberId":
                                predicate = predicate.Or(i => i.MemberId == Convert.ToInt32(searchfor));
                                break;
                        }
                    }

                images = (IOrderedQueryable<AlbumImage>)images.Where(predicate);
            }
            switch (sortby)
            {

                case "id":
                    images = sortDesc == "1" ? images.OrderByDescending(i => i.Id) : images.OrderBy(i => i.Id);

                    break;
                case "date":
                    images = sortDesc == "1" ? images.OrderByDescending(i => i.Timestamp) : images.OrderBy(i => i.Timestamp);
                    break;
                case "desc":
                    images = sortDesc == "1" ? images.OrderByDescending(i => i.Description) : images.OrderBy(i => i.Description);
                    break;
                case "file":
                    images = sortDesc == "1" ? images.OrderByDescending(i => i.Location) : images.OrderBy(i => i.Location);
                    break;
                case "scientific":
                    //I_SCIENTIFICNAME
                    images = sortDesc == "1" ? images.OrderByDescending(i => i.ScientificName) : images.OrderBy(i => i.ScientificName);
                    break;
                case "localname":
                    //I_NORWEGIANNAME
                    images = sortDesc == "1" ? images.OrderByDescending(i => i.CommonName) : images.OrderBy(i => i.CommonName);
                    break;
                case "user":
                    images = sortDesc == "1" ? images.OrderByDescending(i => i.Member.Name) : images.OrderBy(i => i.Member.Name);
                    break;
                case "views":
                    images = sortDesc == "1" ? images.OrderByDescending(i => i.Views) : images.OrderBy(i => i.Views);
                    break;
                case "group":
                    images = sortDesc == "1" ? images.OrderByDescending(i => i.Group.Description) : images.OrderBy(i => i.Group.Description);
                    break;
            }

            return images.ToList();

        }

        private static string GetUniqueFileName(string fileName, out string timestamp)
        {
            fileName = Path.GetFileName(fileName);
            timestamp = DateTime.UtcNow.ToForumDateStr();
            return  timestamp
                    + "_"
                    + GetSafeFilename(fileName);
        }
        private static string GetSafeFilename(string filename)
        {

            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));

        }
        private SelectList GetGroupList()
        {
            SelectListItem selListItem = new() {Value = "0", Text = "All"};

            //Create a list of select list items - this will be returned as your select list
            List<SelectListItem> groupList = new() { selListItem };
            groupList.AddRange(new SelectList(_dbContext.Set<AlbumGroup>(), "Id", "Description"));

            var gList = new SelectList(groupList, "Value", "Text", 0);

            foreach (var item in gList)
            {
                var lname = "Group_" + item.Text.Replace(" ", "");
                if (!String.IsNullOrEmpty(lname))
                    item.Text = _languageResource[lname].Value;
            }
            return gList;
        }

        private ImageMeta MetaData(int id)
        {
            var photo = _dbContext.Set<AlbumImage>().Find(id);
            _dbContext.Entry<AlbumImage>(photo).State = EntityState.Detached;

            var image = Path.Combine(_environment.WebRootPath, _config.ContentFolder, "PhotoAlbum", photo.ImageName);
            FileInfo fileInfo = new FileInfo(image);
            ImageInfo imageInfo = Image.Identify(image);
            ImageMetadata imageMetaData = imageInfo.Metadata;

            ImageMeta imagemeta = new ImageMeta
            {
                Width = imageInfo.Width,
                Height = imageInfo.Height,
                FileSize = fileInfo.Length,
                FileSizeKB = fileInfo.Length / 1024.0,
                Format = imageInfo.Metadata.DecodedImageFormat.Name,
            };
            
            return imagemeta;
        }

    }
}
