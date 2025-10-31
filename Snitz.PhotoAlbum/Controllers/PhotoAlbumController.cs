using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
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
using X.PagedList;


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
        private readonly ISnitzCookie _snitzCookie;
        protected static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public PhotoAlbumController(IWebHostEnvironment hostingEnvironment, PhotoContext dbContext,IMember memberservice,IHtmlLocalizerFactory localizerFactory,ISnitzConfig config,ISnitzCookie snitzCookie)
        {
            _environment = hostingEnvironment;
            _dbContext = dbContext;
            _memberservice = memberservice;
            _config = config;
            _fileProvider = hostingEnvironment.WebRootFileProvider;
            _languageResource = (LanguageService)localizerFactory.Create("SnitzController", "MVCForum");
            _snitzCookie = snitzCookie;

        }
        /// <summary>
        /// Renders the Admin view with an empty <see cref="AdminAlbumViewModel"/>.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> that renders the Admin view.</returns>
        [Authorize(Roles = "Administrator")]
        public IActionResult Admin()
        {
            return View(new AdminAlbumViewModel());
        }
        /// <summary>
        /// Displays a paginated and sortable list of photo albums, filtered by search term if provided.
        /// </summary>
        /// <remarks>The method retrieves photo albums grouped by their owners, including the total number
        /// of images  and the timestamp of the most recent upload for each owner. The results can be sorted and
        /// filtered  based on the provided parameters. Pagination is applied with a fixed page size of 15
        /// items.</remarks>
        /// <param name="page">The page number to display. Defaults to 1.</param>
        /// <param name="sortdir">The sort direction for the list. Accepts "asc" for ascending or "desc" for descending. Defaults to "asc".</param>
        /// <param name="orderby">The field by which to sort the list. Accepts "user", "date", or "count". Defaults to "user".</param>
        /// <param name="term">An optional search term to filter the list by album owner username. Defaults to an empty string.</param>
        /// <returns>An <see cref="IActionResult"/> that renders the "Index" view with a paginated list of photo albums  or the
        /// "Error" view if an exception occurs.</returns>
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
        /// <summary>
        /// Processes a search request based on the provided search criteria and displays the results in an album view.
        /// </summary>
        /// <remarks>This method handles search functionality for species entries. It validates the search
        /// terms, applies sorting and filtering options, and prepares the data for display in the album view. If an
        /// error occurs during processing, an error view is returned with the error message.</remarks>
        /// <param name="vm">The <see cref="SearchViewModel"/> containing the search criteria, including search terms, sorting options,
        /// and filters.</param>
        /// <returns>An <see cref="IActionResult"/> that renders the album view with the search results if the search terms are
        /// valid; otherwise, redirects to the "Album" action if no search terms are provided.</returns>
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
        /// <summary>
        /// Retrieves a photo by its identifier and returns it as a JPEG file.
        /// </summary>
        /// <remarks>The method searches for the photo in the database using the provided identifier. If
        /// the photo exists, it constructs the file path based on the photo's timestamp and location. If the photo is
        /// not found, a default "not found" image is returned instead.</remarks>
        /// <param name="id">The identifier of the photo to retrieve. If <see langword="null"/>, a default "not found" image is returned.</param>
        /// <returns>An <see cref="IActionResult"/> containing the requested photo as a JPEG file if found; otherwise, a default
        /// "not found" image as a JPEG file.</returns>
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
        /// Retrieves the caption (description) of an album image by its identifier.
        /// </summary>
        /// <remarks>If the album image with the specified identifier does not exist, an empty string is
        /// returned. The database context entry for the album image is detached after retrieval.</remarks>
        /// <param name="id">The unique identifier of the album image.</param>
        /// <returns>A <see cref="JsonResult"/> containing the description of the album image if it exists and is not empty;
        /// otherwise, an empty string.</returns>
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
        /// <summary>
        /// Displays a paginated and sortable list of album images for a specified member.
        /// </summary>
        /// <remarks>This action method retrieves album images for a specific member, applying pagination,
        /// sorting, and optional category filtering. If the specified member ID is invalid or not provided, the current
        /// member's ID is used. The method also sets various view-related properties, such as breadcrumb navigation,
        /// ownership status, and sorting options.</remarks>
        /// <param name="id">The identifier of the member whose album images are to be displayed. If null or empty, the current member's
        /// ID is used.</param>
        /// <param name="display">Determines the number of images displayed per page. Valid values are: 1 for 15 images, 2 for 30 images, 3
        /// for 50 images, and 0 (default) for 50 images.</param>
        /// <param name="pagenum">The page number to display. Defaults to 1.</param>
        /// <param name="sortby">The field by which the images are sorted. Defaults to "date".</param>
        /// <param name="sortorder">The order in which the images are sorted. Valid values are "asc" for ascending and "desc" (default) for
        /// descending.</param>
        /// <param name="category">The category filter for the images. Defaults to 0, which means no category filter is applied.</param>
        /// <returns>An <see cref="IActionResult"/> that renders the view displaying the album images. If the member ID is
        /// invalid or not found, the "Error" view is returned.</returns>
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
        /// Displays a paginated and sortable species album based on the specified parameters.
        /// </summary>
        /// <remarks>This action method generates a species album view with pagination, sorting, and
        /// filtering options. The album can be customized based on the provided parameters, such as sorting by a
        /// specific field, filtering by group, and toggling the display of thumbnails.</remarks>
        /// <param name="id">The identifier of the user or entity associated with the album.</param>
        /// <param name="pagenum">The page number to display. Defaults to 1.</param>
        /// <param name="sortby">The field by which the album entries are sorted. Defaults to "date".</param>
        /// <param name="groupFilter">The group filter to apply. Use -1 for no filtering. Defaults to -1.</param>
        /// <param name="sortOrder">The sort order for the album entries. Use "asc" for ascending or "desc" for descending. Defaults to "desc".</param>
        /// <param name="showThumbs">Indicates whether thumbnails should be displayed. Defaults to <see langword="true"/>.</param>
        /// <param name="speciesOnly">Specifies whether to include only species-related entries. Use 1 for species-only entries; otherwise, 0.
        /// Defaults to 1.</param>
        /// <returns>An <see cref="ActionResult"/> that renders the species album view with the specified parameters.</returns>
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
        /// Displays a gallery of images based on the specified parameters.
        /// </summary>
        /// <remarks>The gallery view includes metadata such as the username, image descriptions, and view
        /// counts. The images are retrieved and displayed based on the provided sorting and filtering
        /// parameters.</remarks>
        /// <param name="id">The identifier of the user or entity associated with the gallery. Defaults to 0.</param>
        /// <param name="photoid">The identifier of the currently selected photo. Defaults to 0.</param>
        /// <param name="sortby">The field by which the images should be sorted. Defaults to "date".</param>
        /// <param name="sortOrder">The order in which the images should be sorted. Use <see langword="desc"/> for descending or "1" for
        /// ascending. Defaults to <see langword="desc"/>.</param>
        /// <param name="speciesOnly">A value indicating whether to include only species-related images. Defaults to <see langword="true"/>.</param>
        /// <returns>An <see cref="ActionResult"/> that renders the gallery view with the specified images and metadata.</returns>
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
                             images[0].Timestamp.FromForumDateStr().LocalTime(_snitzCookie) + "<br/>" + images[0].Views +
                             " " + _languageResource.GetString("lblViews");
            ViewBag.title = string.Format("{0}, {1}, {2}", images[0].CommonName, images[0].ScientificName,
                images[0].Description);

            return View(imageFiles);

        }
        /// <summary>
        /// Generates and returns a thumbnail image for the specified album image ID.
        /// </summary>
        /// <remarks>This method retrieves the original image from the database using the provided ID,
        /// resizes it to create a thumbnail, and saves the thumbnail to a designated folder. If the thumbnail already
        /// exists, it is returned directly. The method ensures that the necessary folder structure is created if it
        /// does not already exist.</remarks>
        /// <param name="id">The unique identifier of the album image for which the thumbnail is requested.</param>
        /// <returns>An <see cref="IActionResult"/> containing the thumbnail image as a JPEG file. If the album image does not
        /// exist or an error occurs, a default "not found" image is returned.</returns>
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
        /// <summary>
        /// Renders a partial view for the album upload form with configurable options.
        /// </summary>
        /// <remarks>The method initializes an <see cref="AlbumUploadViewModel"/> with default values,
        /// including  allowed file types and maximum file size, which are retrieved from the application configuration.
        /// The group list is localized based on the current language resource. The partial view  "_popAlbumUpload" is
        /// returned with the populated model.</remarks>
        /// <param name="showall">A boolean value indicating whether all album groups should be displayed.  <see langword="true"/> to display
        /// all groups; otherwise, <see langword="false"/>.</param>
        /// <returns>A <see cref="PartialViewResult"/> containing the album upload form populated with the necessary data.</returns>
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
        /// <summary>
        /// Handles the upload of an album image, saves the file to the server, and stores metadata in the database.
        /// </summary>
        /// <remarks>This method validates the provided model, generates a unique file name for the
        /// uploaded image, saves the image to the server, and creates a new database record for the uploaded image. If
        /// the upload is successful, a JSON response is returned containing the file URL and other details. If an error
        /// occurs during database operations, the error message is included in the JSON response.</remarks>
        /// <param name="model">The <see cref="AlbumUploadViewModel"/> containing the album image and associated metadata.</param>
        /// <returns>An <see cref="IActionResult"/> that represents the result of the upload operation. Returns a JSON object
        /// with the upload status, file URL, and optional caption if the upload is successful. Returns a partial view
        /// with the model if the upload fails validation.</returns>
        public IActionResult Upload(AlbumUploadViewModel model)
        {
            if(!IsImage(model.AlbumImage))
            {
                ModelState.AddModelError("AlbumImage", _languageResource["errInvalidImage"].Value);
                return PartialView("_popAlbumUpload",model);
            }
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
        /// <summary>
        /// Retrieves and displays a paginated list of images associated with a specific member, with options for
        /// filtering, sorting, and categorization.
        /// </summary>
        /// <remarks>This action method supports sorting by multiple fields and allows filtering by
        /// category. It also sets various view-related data, such as the current member ID, display mode, and sorting
        /// options, which are used to configure the view.</remarks>
        /// <param name="id">The unique identifier of the member whose images are to be retrieved.</param>
        /// <param name="display">An integer representing the display mode or view configuration for the images.</param>
        /// <param name="pagenum">The page number to retrieve. Defaults to 1.</param>
        /// <param name="sortby">The field by which the images should be sorted. Valid values include "date", "desc", "id", and "file".
        /// Defaults to "date".</param>
        /// <param name="sortorder">The order in which the images should be sorted. Valid values are "asc" for ascending and "desc" for
        /// descending. Defaults to "desc".</param>
        /// <param name="category">The category ID to filter the images by. If 0, no category filter is applied. Defaults to 0.</param>
        /// <returns>A partial view containing the filtered, sorted, and paginated list of images.</returns>
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
        /// <summary>
        /// Toggles the privacy state of an album image and redirects to the member's image gallery.
        /// </summary>
        /// <remarks>If the specified album image is not found, no changes are made, and the method still
        /// redirects to the "MemberImages" action.</remarks>
        /// <param name="id">The unique identifier of the album image to update.</param>
        /// <param name="memberid">The unique identifier of the member associated with the image.</param>
        /// <param name="state">The desired privacy state of the image. <see langword="true"/> to make the image private; otherwise, <see
        /// langword="false"/>.</param>
        /// <param name="display">An integer representing the display mode or filter to apply when redirecting to the member's image gallery.</param>
        /// <returns>An <see cref="IActionResult"/> that redirects to the "MemberImages" action with the specified member ID and
        /// display parameters.</returns>
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
        /// <summary>
        /// Toggles the "Do Not Feature" state for a specific album image.
        /// </summary>
        /// <param name="id">The unique identifier of the album image to update.</param>
        /// <param name="memberid">The unique identifier of the member associated with the album image.</param>
        /// <param name="state">The new state to set for the "Do Not Feature" flag. <see langword="true"/> to enable; otherwise, <see
        /// langword="false"/>.</param>
        /// <param name="display">The display mode to use when redirecting to the member's images.</param>
        /// <returns>An <see cref="IActionResult"/> that redirects to the "MemberImages" action with the specified member ID and
        /// display mode.</returns>
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
        /// <summary>
        /// Displays the edit form for an album image, allowing the user to modify its details.
        /// </summary>
        /// <remarks>This action retrieves the album image and its associated metadata from the database.
        /// If the image exists, the metadata and member name are passed to the view via <see cref="ViewBag"/>. The form
        /// is pre-populated with the current details of the album image, including its description, group, and
        /// category.</remarks>
        /// <param name="id">The unique identifier of the album image to edit. Must not be <see langword="null"/>.</param>
        /// <param name="display">An optional parameter that determines the display mode. Defaults to 0.</param>
        /// <returns>A partial view containing the edit form for the specified album image, or an error view if the <paramref
        /// name="id"/> is <see langword="null"/>.</returns>
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
        /// <summary>
        /// Updates the details of an existing album image in the database.
        /// </summary>
        /// <remarks>This method retrieves the album image from the database using the ID provided in the
        /// <paramref name="img"/> parameter. If the image exists, its properties are updated with the values from the
        /// view model, and the changes are saved to the database.</remarks>
        /// <param name="img">The view model containing the updated image details and associated metadata.</param>
        /// <returns>A redirection to the "MemberImages" action with the member ID and display mode as route parameters.</returns>
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
        /// <summary>
        /// Updates the details of an existing album group in the database.
        /// </summary>
        /// <remarks>If the specified album group does not exist in the database, no changes are
        /// made.</remarks>
        /// <param name="group">The <see cref="AlbumGroup"/> object containing the updated details. The <see cref="AlbumGroup.Id"/> property
        /// must correspond to an existing album group.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns a content result with the
        /// message "Updated" if the update is successful.</returns>
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
        /// <summary>
        /// Adds a new album group to the database.
        /// </summary>
        /// <remarks>This method requires the caller to be authorized. After the album group is added, 
        /// the database changes are saved immediately. The caller is responsible for ensuring  that the provided
        /// <paramref name="group"/> object contains valid data.</remarks>
        /// <param name="group">The <see cref="AlbumGroup"/> object representing the album group to be added. Cannot be null.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.  Returns a content result with a
        /// message confirming the addition of the group.</returns>
        [HttpPost]
        [Authorize]
        public IActionResult AddGroup(AlbumGroup group)
        {
            _dbContext.Set<AlbumGroup>().Add(group);
            _dbContext.SaveChanges();

            return Content("Group added, Reload page to view/edit groups.");
        }
        /// <summary>
        /// Returns a partial view for adding a new category.
        /// </summary>
        /// <remarks>This action is restricted to authorized users. The returned partial view is named
        /// "_AddCategory".</remarks>
        /// <returns>A <see cref="PartialViewResult"/> that renders the "_AddCategory" partial view.</returns>
        [Authorize]
        public IActionResult AddCategory()
        {
            return PartialView("_AddCategory");
        }
        /// <summary>
        /// Adds a new album category to the database.
        /// </summary>
        /// <remarks>This action requires the user to be authorized. After the category is added, the
        /// database changes are saved immediately.</remarks>
        /// <param name="category">The <see cref="AlbumCategory"/> object representing the category to be added. Cannot be null.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns a success message upon
        /// completion.</returns>
        [HttpPost]
        [Authorize]
        public IActionResult AddCategory(AlbumCategory category)
        {
            _dbContext.Set<AlbumCategory>().Add(category);
            _dbContext.SaveChanges();

            return Content("Category added, Reload page to view.");
        }
        /// <summary>
        /// Deletes an album group with the specified identifier.
        /// </summary>
        /// <remarks>This action requires the user to be authorized. Ensure the specified <paramref
        /// name="id"/> corresponds to an existing album group.</remarks>
        /// <param name="id">The unique identifier of the album group to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.  Returns a plain text response with
        /// the message "Group removed." upon successful deletion.</returns>
        [HttpGet]
        [Authorize]
        public IActionResult DeleteGroup(int id)
        {
            _dbContext.Set<AlbumGroup>().Where(g => g.Id == id).ExecuteDelete();

            return Content("Group removed.");
        }
        /// <summary>
        /// Deletes an image from the database and redirects to the MemberImages view.
        /// </summary>
        /// <param name="id">The unique identifier of the image to delete.</param>
        /// <param name="memberid">The unique identifier of the member associated with the image.</param>
        /// <param name="display">An integer value used to determine the display state in the redirection.</param>
        /// <returns>An <see cref="IActionResult"/> that redirects to the MemberImages view with the specified member ID and
        /// display state.</returns>
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
        /// <summary>
        /// Deletes the specified album image from the database and redirects to the Member view.
        /// </summary>
        /// <remarks>If the specified album image does not exist in the database, no action is
        /// performed.</remarks>
        /// <param name="img">The album image to delete. The <see cref="AlbumImage.Id"/> property must be set to identify the image.</param>
        /// <param name="display">An optional parameter specifying the display mode for the Member view. Defaults to 1.</param>
        /// <returns>A redirection to the Member view with the specified member ID and display mode.</returns>
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

        /// <summary>
        /// Creates a paginated list of albums based on the specified page number, page size, sorting order, and sorting
        /// criteria.
        /// </summary>
        /// <remarks>The method supports sorting by username, last upload date, or image count. If the
        /// <paramref name="sortBy"/> value is invalid, the method returns <see langword="null"/>.</remarks>
        /// <param name="pagenum">The number of the page to retrieve. Must be a positive integer.</param>
        /// <param name="pagesize">The number of items per page. Must be a positive integer.</param>
        /// <param name="sortOrder">The order in which to sort the results. Acceptable values are <see langword="asc"/> for ascending or <see
        /// langword="desc"/> for descending.</param>
        /// <param name="sortBy">The property by which to sort the results. Acceptable values are "user", "date", or "count".</param>
        /// <param name="model">The queryable collection of albums to paginate and sort. Cannot be null or empty.</param>
        /// <returns>A <see cref="PagedList{T}"/> containing the paginated and sorted albums, or <see langword="null"/> if the
        /// provided <paramref name="model"/> is empty.</returns>
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
        /// <summary>
        /// Resizes the image to create a thumbnail based on the specified size and configuration settings.
        /// </summary>
        /// <remarks>The resizing behavior is determined by the configuration value for "STRTHUMBTYPE": -
        /// If the value is "scaled", the height is set to 0, and the image is resized with padding to maintain the
        /// aspect ratio. - Otherwise, the image is resized using cropping to fit the specified size.</remarks>
        /// <param name="context">The image processing context to apply the resizing operation to.</param>
        /// <param name="size">The target size for the thumbnail. The width is always used, and the height may be adjusted  based on the
        /// configuration settings.</param>
        /// <returns>An <see cref="IImageProcessingContext"/> representing the modified image processing context  after the
        /// resizing operation.</returns>
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
        private bool IsImage(IFormFile file)
        {
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    var image = SixLabors.ImageSharp.Image.Load(stream);
                    return true; // Successfully loaded as an image
                }
            }
            catch
            {
                return false; // Not an image
            }
        }
    }
}
