using System.Configuration;
using System.Drawing.Imaging;
using System.Security.Authentication;
using System.Text;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Snitz.PhotoAlbum.Models
{
    public class PhotoRepository : IDisposable
    {
        //private List<AlbumImage> _data;
        private int _memberid;
        public long Pagecount;
        public int Pagesize;
        private readonly ISnitzConfig _snitzconfig;
        private readonly SnitzDbContext _dbContext;

        public PhotoRepository(ISnitzConfig config,SnitzDbContext snitzdb,int memberid=0, int pagesize = 30)
        {
            _memberid = memberid;
            Pagesize = pagesize;
            _snitzconfig = config;
            _dbContext = snitzdb;
            
            //if(memberid >= 0)
            //    Initialize();
        }
        //private void Initialize()
        //{

        //    string tablePrefix = ConfigurationManager.AppSettings["forumTablePrefix"];
        //    string memberTablePrefix = ConfigurationManager.AppSettings["memberTablePrefix"];

        //    using (var context = new SnitzDataContext())
        //    {
        //        _data = new List<AlbumImage>();
        //        Sql sql = new Sql();
        //        var concat = "im.I_DATE + '_' + I_LOC";
        //        if (context.dbtype == "mysql")
        //            concat = "CONCAT(im.I_DATE, '_' , I_LOC)";
        //        sql.Select("im.*, M.M_NAME AS MemberName, G.O_GROUP_NAME AS GroupName, " + concat + " AS ImageName");
        //        sql.From(tablePrefix + "IMAGES im");
        //        sql.LeftJoin(memberTablePrefix + "MEMBERS M").On("M.MEMBER_ID = im.I_MID");
        //        sql.LeftJoin(tablePrefix + "ORG_GROUP G").On("G.O_GROUP_ID = im.I_GROUP_ID");
        //        if (_memberid > 0)
        //        {
        //            sql.Where("I_MID=@0", _memberid);
        //            if (_memberid != WebSecurity.CurrentUserId)
        //            {
        //                if (!Roles.IsUserInRole("Administrator"))
        //                    sql.Where("I_PRIVATE != 1");
        //            }
        //        }
        //        else
        //        {
        //            sql.Where("I_PRIVATE != 1");
        //        }


        //        _data = context.Query<AlbumImage>(sql).ToList();

        //    }
        //    Pagecount = _data.Count/Pagesize;

        //    if ((_data.Count % Pagesize) != 0)
        //        Pagecount++;
        //}

        public IEnumerable<int> PhotoIdList()
        {
            return _dbContext.Database.SqlQuery<int>($"SELECT I_ID FROM FORUM_IMAGES WHERE I_PRIVATE=0 AND I_NOTFEATURED=0");


        } 

        public List<AlbumGroup> AlbumGroups()
        {
            return _dbContext.Database.SqlQuery<AlbumGroup>($"ORDER BY O_GROUP_ORDER").ToList();

        } 
        public List<AlbumList> AlbumIndex(int pagenum)
        {
            string tablePrefix = ConfigurationManager.AppSettings["forumTablePrefix"];
            string memberTablePrefix = ConfigurationManager.AppSettings["memberTablePrefix"];
            var data = new List<AlbumList>();
            Sql sql = new Sql();
            sql.Select("a.MEMBER_ID, a.M_NAME, Count(b.I_ID) AS imgCount, Max(b.I_DATE) AS imgLastUpload");
            sql.From(memberTablePrefix + "MEMBERS a");
            sql.InnerJoin(tablePrefix + "IMAGES b").On("a.MEMBER_ID = b.I_MID");
            sql.GroupBy(new object[]{" a.M_NAME", "a.MEMBER_ID"});
            sql.OrderBy("a.M_NAME");
            
            
            //using (var context = new SnitzDataContext())
            //{
            //    if(context.dbtype=="mysql")
            //        sql.Append(" COLLATE utf8mb4_danish_ci");
            //    else
            //        sql.Append(" COLLATE Danish_Norwegian_CI_AS");
                data = _dbContext.Database.SqlQuery<AlbumList>(sql).Skip((pagenum-1)*Pagesize).Take(Pagesize).ToList();

            //}
            Pagecount = data.Count / Pagesize;

            if ((data.Count % Pagesize) != 0)
                Pagecount++;
            return data;
        }
        public AlbumImage GetEntryById(int id)
        {
            string tablePrefix = _snitzconfig. ConfigurationManager.AppSettings["forumTablePrefix"];
            string memberTablePrefix = ConfigurationManager.AppSettings["memberTablePrefix"];
            using (var context = new SnitzDataContext())
            {
                Sql sql = new Sql();
                var concat = "im.I_DATE + '_' + im.I_LOC";
                if (context.dbtype == "mysql")
                    concat = "CONCAT(im.I_DATE, '_' , im.I_LOC)";
                sql.Select("im.*, M.M_NAME AS MemberName, G.O_GROUP_NAME AS GroupName, " + concat + " AS ImageName");
                sql.From(tablePrefix + "IMAGES im");
                sql.LeftJoin(memberTablePrefix + "MEMBERS M").On("M.MEMBER_ID = im.I_MID");
                sql.LeftJoin(tablePrefix + "ORG_GROUP G").On("G.O_GROUP_ID = im.I_GROUP_ID");
                sql.Where("I_ID=@0", id);
                AlbumImage b = context.FirstOrDefault<AlbumImage>(sql);
                return b;
            }

        }

        public List<AlbumImage> GetSpeciesEntries(int pagenum, string sortby = "id", int filter = 0, bool speciesOnly = true, List<string> searchin = null, string searchfor = null, string sortDesc="")
        {
            
            Sql sql = new Sql();
            using (var db = new SnitzDataContext())
            {
                var concat = "im.I_DATE + '_' + im.I_LOC";
                if (db.dbtype == "mysql")
                    concat = "CONCAT(im.I_DATE, '_' , im.I_LOC)";
                sql.Select("im.*, COALESCE(M.M_NAME,'n/a') AS MemberName, G.O_GROUP_NAME AS GroupName, " + concat + " AS ImageName");
                sql.From(db.ForumTablePrefix + "IMAGES im");
                sql.LeftJoin(db.MemberTablePrefix + "MEMBERS M").On("M.MEMBER_ID = im.I_MID");
                sql.LeftJoin(db.ForumTablePrefix + "ORG_GROUP G").On("G.O_GROUP_ID = im.I_GROUP_ID");
                
                if (searchin != null && searchin.Contains("Member"))
                {
                    if (WebSecurity.GetUserId(searchfor) != WebSecurity.CurrentUserId)
                    {
                        sql.Where("im.I_PRIVATE != 1");
                    }
                }
                else if (searchin != null && searchin.Contains("MemberId"))
                {
                    if (Convert.ToInt32(searchfor) != WebSecurity.CurrentUserId)
                    {
                        sql.Where("im.I_PRIVATE != 1");
                    }
                }
                else { sql.Where("im.I_PRIVATE != 1"); }

                if (speciesOnly)
                {
                    sql.Where("(im.I_SCIENTIFICNAME != '' AND im.I_SCIENTIFICNAME IS NOT NULL)");
                    sql.Where("(im.I_NORWEGIANNAME != '' AND im.I_NORWEGIANNAME IS NOT NULL)");
                    sql.Where("(im.I_DESC != '' AND im.I_DESC IS NOT NULL)");
                    //data =
                    //    data.Where(d => !String.IsNullOrWhiteSpace(d.ScientificName) && !String.IsNullOrEmpty(d.CommonName) && !String.IsNullOrEmpty(d.Description))
                    //        .ToList();
                }

            if (filter > 0)
            {
                sql.Where("im.I_GROUP_ID = @0", filter);
                //data = data.Where(d => d.Group ==  filter).ToList();
            }
            if (searchfor != null)
            {
                StringBuilder res = new StringBuilder();
                if (searchin != null)
                    foreach (string s in searchin)
                    {
                        switch (s)
                        {
                                case "Id":
                                    res.AppendFormat(res.Length == 0 ? "(im.I_ID LIKE {0})" : "OR (im.I_ID LIKE {0}) ", "'%" + searchfor + "%'");

                                    //res = res == null ? data.Where(d => d.Description.Contains(searchfor, CompareOptions.IgnoreCase)) : _data.Where(d =>d.Description.Contains(searchfor, CompareOptions.IgnoreCase)).Union(res);
                                    break;
                                case "Desc":
                                    res.AppendFormat(res.Length == 0 ? "(im.I_DESC LIKE {0})" : "OR (im.I_DESC LIKE {0}) ", "'%" + searchfor + "%'");

                                //res = res == null ? data.Where(d => d.Description.Contains(searchfor, CompareOptions.IgnoreCase)) : _data.Where(d =>d.Description.Contains(searchfor, CompareOptions.IgnoreCase)).Union(res);
                                break;
                            case "CommonName":
                                    res.AppendFormat(res.Length == 0 ? "(im.I_NORWEGIANNAME LIKE {0})" : "OR (im.I_NORWEGIANNAME LIKE {0}) ", "'%" + searchfor + "%'");
                                    //res = res == null ? data.Where(d => d.CommonName.Contains(searchfor, CompareOptions.IgnoreCase)) : _data.Where(d =>d.CommonName.Contains(searchfor, CompareOptions.IgnoreCase)).Union(res);
                                break;
                            case "ScientificName":
                                    res.AppendFormat(res.Length == 0 ? "(im.I_SCIENTIFICNAME LIKE {0})" : "OR (im.I_SCIENTIFICNAME LIKE {0}) ", "'%" + searchfor + "%'");
                                    //res = res == null ? data.Where(d => d.ScientificName.Contains(searchfor, CompareOptions.IgnoreCase)) : _data.Where(d =>d.ScientificName.Contains(searchfor, CompareOptions.IgnoreCase)).Union(res);
                                break;
                            case "Member":
                                    res.AppendFormat(res.Length == 0 ? "(M.M_NAME LIKE {0})" : "OR (M.M_NAME LIKE {0}) ", "'%" + searchfor + "%'");
                                    //res = res == null ? data.Where(d => d.MemberName.Contains(searchfor, CompareOptions.IgnoreCase)) : _data.Where(d =>d.MemberName.Contains(searchfor, CompareOptions.IgnoreCase)).Union(res);
                                break;
                                case "MemberId":
                                    res.AppendFormat(res.Length == 0 ? "(M.MEMBER_ID = {0})" : "OR (M.MEMBER_ID = {0}) ",  searchfor );
                                    //res = res == null ? data.Where(d => d.MemberName.Contains(searchfor, CompareOptions.IgnoreCase)) : _data.Where(d =>d.MemberName.Contains(searchfor, CompareOptions.IgnoreCase)).Union(res);
                                break;
                            }

                        }
                if (res.Length > 0) sql.Where(res.ToString());
            }
            switch (sortby)
            {
                
                case "id":
                    sql.OrderBy(sortDesc == "1" ? "im.I_ID DESC" : "im.I_ID");
                    break;
                case "date":
                    sql.OrderBy(sortDesc == "1" ? "im.I_DATE DESC" : "im.I_DATE");
                    //data = sortDesc == "1" ? _data.OrderByDescending(d => d.Timestamp).ToList() : _data.OrderBy(d => d.Timestamp).ToList();
                    break;
                case "desc":
                    sql.OrderBy(sortDesc == "1" ? "im.I_DESC DESC" : "im.I_DESC");
                    //data = sortDesc == "1" ? _data.OrderByDescending(d => d.Description, StringComparer.CurrentCulture).ToList() : _data.OrderBy(d => d.Description, StringComparer.CurrentCulture).ToList();
                    break;
                case "file":
                    sql.OrderBy(sortDesc == "1" ? "im.I_LOC DESC" : "im.I_LOC");
                    //data = sortDesc == "1" ? _data.OrderByDescending(d => d.Location, StringComparer.CurrentCulture).ToList() : _data.OrderBy(d => d.Location, StringComparer.CurrentCulture).ToList();
                    break;
                case "scientific":
                    //I_SCIENTIFICNAME
                    sql.OrderBy(sortDesc == "1" ? "im.I_SCIENTIFICNAME DESC" : "im.I_SCIENTIFICNAME");
                    //data = sortDesc == "1" ? _data.OrderByDescending(d => d.ScientificName, StringComparer.CurrentCulture).ToList() : _data.OrderBy(d => d.ScientificName, StringComparer.CurrentCulture).ToList();
                    break;
                case "localname":
                    //I_NORWEGIANNAME
                    sql.OrderBy(sortDesc == "1" ? "im.I_NORWEGIANNAME DESC" : "im.I_NORWEGIANNAME");
                    //data = sortDesc == "1" ? _data.OrderByDescending(d => d.CommonName, StringComparer.CurrentCulture).ToList() : _data.OrderBy(d => d.CommonName, StringComparer.CurrentCulture).ToList();
                    break;
                case "user":
                    sql.OrderBy(sortDesc == "1" ? "M.M_NAME DESC" : "M.M_NAME");
                    //data = sortDesc == "1" ? _data.OrderByDescending(d => d.MemberName, StringComparer.CurrentCulture).ToList() : _data.OrderBy(d => d.MemberName, StringComparer.CurrentCulture).ToList();
                    break;
                case "views":
                    sql.OrderBy(sortDesc == "1" ? "im.I_VIEWS DESC" : "im.I_VIEWS");
                    //data = sortDesc == "1" ? _data.OrderByDescending(d => d.Views).ToList() : _data.OrderBy(d => d.Views).ToList();
                    break;
                case "group":
                    sql.OrderBy(sortDesc == "1" ? "G.O_GROUP_NAME DESC" : "G.O_GROUP_NAME");
                    //data = sortDesc == "1" ? _data.OrderByDescending(d => d.GroupName, StringComparer.CurrentCulture).ToList() : _data.OrderBy(d => d.GroupName, StringComparer.CurrentCulture).ToList();
                    break;
            }
                var data = db.Page<AlbumImage>(pagenum, Pagesize,sql);
                Pagecount = data.TotalPages;

                return data.Items;
            }

        }

        public void Dispose()
        {
            //if (_data != null)
            //{
            //    _data.Clear();
            //    _data = null;
            //}
        }

        public void DeleteImages(IEnumerable<int> images)
        {
            using (var context = new SnitzDataContext())
            {
                var locs = context.Fetch<AlbumImage>("WHERE I_ID IN (" + string.Join(", ", images.Select(n => n.ToString()).ToArray()) + ")");

                foreach (var albumImage in locs)
                {
                    if (WebSecurity.CurrentUserId != albumImage.MemberId && !WebSecurity.IsAdministrator)
                    {
                        throw new AuthenticationException("Can not do this");
                    }
                    File.Delete(HttpContext.Current.Server.MapPath("~/" + albumImage.RootFolder + "/PhotoAlbum/" + albumImage.Timestamp + "_" + albumImage.Location));
                    File.Delete(HttpContext.Current.Server.MapPath("~/" + albumImage.RootFolder + "/PhotoAlbum/thumbs/" + albumImage.Timestamp + "_" + albumImage.Location));
                    context.Delete(albumImage);
                }
            }
        }

        public AlbumImage GetImage(int id)
        {

            using (var context = new SnitzDataContext())
            {
                var concat = "im.I_DATE + '_' + im.I_LOC";
                if (context.dbtype == "mysql")
                    concat = "CONCAT(im.I_DATE, '_' , im.I_LOC)";
                Sql sql = new Sql();
                sql.Select("im.*, " + concat + " AS ImageName");
                sql.From(context.ForumTablePrefix +"IMAGES im");
                sql.Where("im.I_ID=@0", id);
                return context.SingleOrDefault<AlbumImage>(sql);

            }
        }


        public int AddAPIImage(AlbumImage image)
        {
            var logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            string tablePrefix = ConfigurationManager.AppSettings["forumTablePrefix"];
            string albumFolder = HttpContext.Current.Server.MapPath("~/" + image.RootFolder + "/PhotoAlbum/");
            if (!Directory.Exists(albumFolder))
            {
                Directory.CreateDirectory(albumFolder);
            }
            using (var context = new SnitzDataContext())
            {
                context.Save(tablePrefix + "IMAGES", "I_ID", image);

            }

            //Save the image to disk
            logger.Debug("Save the image to disk");
            if (image.Img != null)
            {
                ImageFormat output = ImageFormat.Jpeg;
                string fullFileName = HttpContext.Current.Server.MapPath("~/" + image.RootFolder + "/PhotoAlbum/" + image.Timestamp + "_" + image.Location);
                switch (Path.GetExtension(fullFileName))
                {
                    case ".bmp":
                        output = ImageFormat.Bmp;
                        break;
                    case ".png":
                        output = ImageFormat.Png;
                        break;
                    case ".gif":
                        output = ImageFormat.Gif;
                        break;
                    case ".ico":
                        output = ImageFormat.Icon;
                        break;

                    default:
                        break;
                }
                ImageHelper.RotateImageByExifOrientationData(fullFileName, "", output, true);

                logger.Debug("Save the rotated image to disk " + fullFileName);
                image.Img.Save(fullFileName);
                image.Img.Dispose();
            }
            image = GetImage(image.Id);
            image.GenerateThumbnail();
            logger.Debug("Return " + image.Id);
            return image.Id;
        }
        public int AddImage(AlbumImage image)
        {
            string tablePrefix = ConfigurationManager.AppSettings["forumTablePrefix"];
            string albumFolder = HttpContext.Current.Server.MapPath("~/" + image.RootFolder + "/PhotoAlbum/");
            if (!Directory.Exists(albumFolder))
            {
                Directory.CreateDirectory(albumFolder);
            }
            using (var context = new SnitzDataContext())
            {
                context.Save(tablePrefix + "IMAGES", "I_ID", image);
            }

            //Save the image to disk
            if (image.Location != null) //if (image.Img != null)
            {
                ImageFormat output = ImageFormat.Jpeg;

                string fullFileName = HttpContext.Current.Server.MapPath("~/" + image.RootFolder + "/PhotoAlbum/" + image.Timestamp + "_" + image.Location);
                switch (Path.GetExtension(fullFileName))
                {
                    case ".bmp" :
                        output = ImageFormat.Bmp;
                        break;
                    case ".png":
                        output = ImageFormat.Png;
                        break;
                    case ".gif":
                        output = ImageFormat.Gif;
                        break;
                    case ".ico":
                        output = ImageFormat.Icon;
                        break;

                    default:
                        break;
                }
                ImageHelper.RotateImageByExifOrientationData(fullFileName, "", output,true);
                //image.Img.Save(fullFileName);
                //image.Img.Dispose();
            }
            image = GetImage(image.Id);
            image.GenerateThumbnail();
            return image.Id;
        }

        public static void SaveImage(AlbumImage image)
        {


            using (var context = new SnitzDataContext())
            {
                context.Update(image);
            }
            //Save the image to disk
            if (image.Img != null)
            {
                ImageFormat output = ImageFormat.Jpeg;
                string fullFileName = HttpContext.Current.Server.MapPath("~/" + image.RootFolder + "/PhotoAlbum/" + image.ImageName);
                switch (Path.GetExtension(fullFileName))
                {
                    case ".bmp":
                        output = ImageFormat.Bmp;
                        break;
                    case ".png":
                        output = ImageFormat.Png;
                        break;
                    case ".gif":
                        output = ImageFormat.Gif;
                        break;
                    case ".ico":
                        output = ImageFormat.Icon;
                        break;

                    default:
                        break;
                }
                ImageHelper.RotateImageByExifOrientationData(fullFileName, "", output, true);
                //ImageHelper.RotateImageByExifOrientationData(image.Img, true);
                image.Img.Save(fullFileName);
                image.GenerateThumbnail();
                image.Img.Dispose();
            }
        }

        public void Save(AlbumGroup model)
        {
            using (var context = new SnitzDataContext())
            {
                context.Update(model);

            }
        }
        public void Add(AlbumGroup model)
        {
            string tablePrefix = ConfigurationManager.AppSettings["forumTablePrefix"];
            using (var context = new SnitzDataContext())
            {
                context.Save(tablePrefix + "ORG_GROUP", "O_GROUP_ID", model);
            }
        }

        public void Delete(AlbumGroup model)
        {
            using (var context = new SnitzDataContext())
            {
                context.Delete(model);
            }
        }

        public static void SetPrivacy(int id)
        {
            using (var context = new SnitzDataContext())
            {
                var img = context.SingleOrDefault<AlbumImage>("WHERE I_ID=@0", new[] { id });
                if (WebSecurity.CurrentUserId != img.MemberId && !WebSecurity.IsAdministrator)
                {
                    throw new AuthenticationException("Can not do this");
                }
                context.Update<AlbumImage>("SET I_PRIVATE=I_PRIVATE^1 WHERE I_ID=@0", id);
            }
        }
        public static void SetDoNotFeature(int id)
        {
            using (var context = new SnitzDataContext())
            {
                var img = context.SingleOrDefault<AlbumImage>("WHERE I_ID=@0", new[] { id });
                if (WebSecurity.CurrentUserId != img.MemberId && !WebSecurity.IsAdministrator)
                {
                    throw new AuthenticationException("Can not do this");

                }
                context.Update<AlbumImage>("SET I_NOTFEATURED=I_NOTFEATURED^1 WHERE I_ID=@0", id);
            }
        }

        public static void UpdateViews(AlbumImage photo)
        {
            using (var context = new SnitzDataContext())
            {
                context.Update<AlbumImage>("SET I_VIEWS=I_VIEWS+1 WHERE I_ID=@0", photo.Id);
            }
        }

        public Page<AlbumList> AlbumIndex(int pagenum, string sortOrder, string sortCol, string term = "")
        {
            string tablePrefix = ConfigurationManager.AppSettings["forumTablePrefix"];
            string memberTablePrefix = ConfigurationManager.AppSettings["memberTablePrefix"];
            
            Sql sql = new Sql();
            sql.Select("a.MEMBER_ID, a.M_NAME, Count(b.I_ID) AS imgCount, Max(b.I_DATE) AS imgLastUpload ");
            sql.From(memberTablePrefix + "MEMBERS a ");
            sql.InnerJoin(tablePrefix + "IMAGES b ").On("a.MEMBER_ID = b.I_MID ");
            if (!String.IsNullOrWhiteSpace(term))
            {
                sql.Where(" a.M_NAME LIKE '" + term + "%'");
            }

            sql.Where("a.M_STATUS=1");
            sql.GroupBy(new object[] { " a.M_NAME", "a.MEMBER_ID" });
            string dir = sortOrder == "desc" ? " DESC" : "";
            using (var context = new SnitzDataContext())
            {
                switch (sortCol)
                {
                    case "user":
                            if (context.dbtype == "mysql")
                                sql.OrderBy("a.M_NAME COLLATE utf8mb4_danish_ci" + dir);
                            else
                                sql.OrderBy("a.M_NAME COLLATE Danish_Norwegian_CI_AS" + dir);
                        break;
                    case "count":
                        sql.OrderBy("Count(b.I_ID) " + dir);
                        break;
                    case "date":
                        sql.OrderBy("Max(b.I_DATE) " + dir);
                        break;
                    default:
                            if (context.dbtype == "mysql")
                                sql.OrderBy("a.M_NAME COLLATE utf8mb4_danish_ci" + dir);
                            else
                                sql.OrderBy("a.M_NAME COLLATE Danish_Norwegian_CI_AS" + dir);
                        break;
                }

                Sql sqlcount = new Sql("SELECT COUNT(DISTINCT I_MID) as albums FROM FORUM_IMAGES");
                if (!String.IsNullOrWhiteSpace(term))
                {
                    sqlcount = new Sql("SELECT COUNT(DISTINCT b.I_MID) as albums");
                    sqlcount.From(tablePrefix + "IMAGES b ");
                    sqlcount.InnerJoin(memberTablePrefix + "MEMBERS m ").On("b.I_MID = m.MEMBER_ID ");
                    sqlcount.Where(" m.M_NAME LIKE '"+ term + "%'");
                }
                return context.Page<AlbumList>(pagenum,Pagesize, sql, sqlcount.SQL);

            }

        }

        public int FindImagePage(int currentid, string sortby = "id", int filter = 0, bool speciesOnly = true, List<string> searchin = null, string searchfor = null, string sortDesc = "")
        {

            Sql sql = new Sql();
            using (var db = new SnitzDataContext())
            {
                var concat = "im.I_DATE + '_' + im.I_LOC";
                if (db.dbtype == "mysql")
                    concat = "CONCAT(im.I_DATE, '_' , im.I_LOC)";
                sql.Select("im.I_ID");
                sql.From(db.ForumTablePrefix + "IMAGES im");
                sql.LeftJoin(db.MemberTablePrefix + "MEMBERS M").On("M.MEMBER_ID = im.I_MID");
                sql.LeftJoin(db.ForumTablePrefix + "ORG_GROUP G").On("G.O_GROUP_ID = im.I_GROUP_ID");

                if (searchin != null && searchin.Contains("Member"))
                {
                    if (WebSecurity.GetUserId(searchfor) != WebSecurity.CurrentUserId)
                    {
                        sql.Where("im.I_PRIVATE != 1");
                    }
                }
                else if (searchin != null && searchin.Contains("MemberId"))
                {
                    if (Convert.ToInt32(searchfor) != WebSecurity.CurrentUserId)
                    {
                        sql.Where("im.I_PRIVATE != 1");
                    }
                }
                else { sql.Where("im.I_PRIVATE != 1"); }

                if (speciesOnly)
                {
                    sql.Where("(im.I_SCIENTIFICNAME != '' AND im.I_SCIENTIFICNAME IS NOT NULL)");
                    sql.Where("(im.I_NORWEGIANNAME != '' AND im.I_NORWEGIANNAME IS NOT NULL)");
                    sql.Where("(im.I_DESC != '' AND im.I_DESC IS NOT NULL)");
                    //data =
                    //    data.Where(d => !String.IsNullOrWhiteSpace(d.ScientificName) && !String.IsNullOrEmpty(d.CommonName) && !String.IsNullOrEmpty(d.Description))
                    //        .ToList();
                }

                if (filter > 0)
                {
                    sql.Where("im.I_GROUP_ID = @0", filter);
                    //data = data.Where(d => d.Group ==  filter).ToList();
                }
                if (searchfor != null)
                {
                    StringBuilder res = new StringBuilder();
                    if (searchin != null)
                        foreach (string s in searchin)
                        {
                            switch (s)
                            {
                                case "Id":
                                    res.AppendFormat(res.Length == 0 ? "(im.I_ID LIKE {0})" : "OR (im.I_ID LIKE {0}) ", "'%" + searchfor + "%'");

                                    //res = res == null ? data.Where(d => d.Description.Contains(searchfor, CompareOptions.IgnoreCase)) : _data.Where(d =>d.Description.Contains(searchfor, CompareOptions.IgnoreCase)).Union(res);
                                    break;
                                case "Desc":
                                    res.AppendFormat(res.Length == 0 ? "(im.I_DESC LIKE {0})" : "OR (im.I_DESC LIKE {0}) ", "'%" + searchfor + "%'");

                                    //res = res == null ? data.Where(d => d.Description.Contains(searchfor, CompareOptions.IgnoreCase)) : _data.Where(d =>d.Description.Contains(searchfor, CompareOptions.IgnoreCase)).Union(res);
                                    break;
                                case "CommonName":
                                    res.AppendFormat(res.Length == 0 ? "(im.I_NORWEGIANNAME LIKE {0})" : "OR (im.I_NORWEGIANNAME LIKE {0}) ", "'%" + searchfor + "%'");
                                    //res = res == null ? data.Where(d => d.CommonName.Contains(searchfor, CompareOptions.IgnoreCase)) : _data.Where(d =>d.CommonName.Contains(searchfor, CompareOptions.IgnoreCase)).Union(res);
                                    break;
                                case "ScientificName":
                                    res.AppendFormat(res.Length == 0 ? "(im.I_SCIENTIFICNAME LIKE {0})" : "OR (im.I_SCIENTIFICNAME LIKE {0}) ", "'%" + searchfor + "%'");
                                    //res = res == null ? data.Where(d => d.ScientificName.Contains(searchfor, CompareOptions.IgnoreCase)) : _data.Where(d =>d.ScientificName.Contains(searchfor, CompareOptions.IgnoreCase)).Union(res);
                                    break;
                                case "Member":
                                    res.AppendFormat(res.Length == 0 ? "(M.M_NAME LIKE {0})" : "OR (M.M_NAME LIKE {0}) ", "'%" + searchfor + "%'");
                                    //res = res == null ? data.Where(d => d.MemberName.Contains(searchfor, CompareOptions.IgnoreCase)) : _data.Where(d =>d.MemberName.Contains(searchfor, CompareOptions.IgnoreCase)).Union(res);
                                    break;
                                case "MemberId":
                                    res.AppendFormat(res.Length == 0 ? "(M.MEMBER_ID = {0})" : "OR (M.MEMBER_ID = {0}) ", searchfor);
                                    //res = res == null ? data.Where(d => d.MemberName.Contains(searchfor, CompareOptions.IgnoreCase)) : _data.Where(d =>d.MemberName.Contains(searchfor, CompareOptions.IgnoreCase)).Union(res);
                                    break;
                            }

                        }
                    if (res.Length > 0) sql.Where(res.ToString());
                }
                switch (sortby)
                {

                    case "id":
                        sql.OrderBy(sortDesc == "1" ? "im.I_ID DESC" : "im.I_ID");
                        break;
                    case "date":
                        sql.OrderBy(sortDesc == "1" ? "im.I_DATE DESC" : "im.I_DATE");
                        //data = sortDesc == "1" ? _data.OrderByDescending(d => d.Timestamp).ToList() : _data.OrderBy(d => d.Timestamp).ToList();
                        break;
                    case "desc":
                        sql.OrderBy(sortDesc == "1" ? "im.I_DESC DESC" : "im.I_DESC");
                        //data = sortDesc == "1" ? _data.OrderByDescending(d => d.Description, StringComparer.CurrentCulture).ToList() : _data.OrderBy(d => d.Description, StringComparer.CurrentCulture).ToList();
                        break;
                    case "file":
                        sql.OrderBy(sortDesc == "1" ? "im.I_LOC DESC" : "im.I_LOC");
                        //data = sortDesc == "1" ? _data.OrderByDescending(d => d.Location, StringComparer.CurrentCulture).ToList() : _data.OrderBy(d => d.Location, StringComparer.CurrentCulture).ToList();
                        break;
                    case "scientific":
                        //I_SCIENTIFICNAME
                        sql.OrderBy(sortDesc == "1" ? "im.I_SCIENTIFICNAME DESC" : "im.I_SCIENTIFICNAME");
                        //data = sortDesc == "1" ? _data.OrderByDescending(d => d.ScientificName, StringComparer.CurrentCulture).ToList() : _data.OrderBy(d => d.ScientificName, StringComparer.CurrentCulture).ToList();
                        break;
                    case "localname":
                        //I_NORWEGIANNAME
                        sql.OrderBy(sortDesc == "1" ? "im.I_NORWEGIANNAME DESC" : "im.I_NORWEGIANNAME");
                        //data = sortDesc == "1" ? _data.OrderByDescending(d => d.CommonName, StringComparer.CurrentCulture).ToList() : _data.OrderBy(d => d.CommonName, StringComparer.CurrentCulture).ToList();
                        break;
                    case "user":
                        sql.OrderBy(sortDesc == "1" ? "M.M_NAME DESC" : "M.M_NAME");
                        //data = sortDesc == "1" ? _data.OrderByDescending(d => d.MemberName, StringComparer.CurrentCulture).ToList() : _data.OrderBy(d => d.MemberName, StringComparer.CurrentCulture).ToList();
                        break;
                    case "views":
                        sql.OrderBy(sortDesc == "1" ? "im.I_VIEWS DESC" : "im.I_VIEWS");
                        //data = sortDesc == "1" ? _data.OrderByDescending(d => d.Views).ToList() : _data.OrderBy(d => d.Views).ToList();
                        break;
                    case "group":
                        sql.OrderBy(sortDesc == "1" ? "G.O_GROUP_NAME DESC" : "G.O_GROUP_NAME");
                        //data = sortDesc == "1" ? _data.OrderByDescending(d => d.GroupName, StringComparer.CurrentCulture).ToList() : _data.OrderBy(d => d.GroupName, StringComparer.CurrentCulture).ToList();
                        break;
                }
                var data = db.Query<int>( sql);
                
                var test = data.ToList();

                return test.FindIndex(c => c == currentid);
            }

        }

    }

}