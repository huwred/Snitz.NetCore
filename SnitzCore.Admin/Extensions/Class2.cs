using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnitzCore.BackOffice.Extensions
{
    public static class Class2
    {
        public static HtmlString ShowRankImages(this HtmlHelper helper, string selectedImage, int key)
        {
            //var physicalPath = HttpContext.Current.Server.MapPath("~/Content/rankimages");
            //var files = Directory.GetFiles(physicalPath, "*.gif");
            string images = "";
            ////foreach (string file in files)
            ////{
            ////    var imagefile = Path.GetFileName(file);
            ////    var test = VirtualPathUtility.ToAbsolute(string.Format(@"~/Content/rankimages/{0}", imagefile));
            ////    var tag = new TagBuilder("img");
            ////    tag.Attributes.Add("src", test);
            ////    tag.AddCssClass("rank");
            ////    tag.Attributes.Add("title", Path.GetFileName(file));
            ////    tag.Attributes.Add("name", key.ToString());
            ////    if (!String.IsNullOrWhiteSpace(selectedImage) && file.EndsWith(selectedImage))
            ////        tag.AddCssClass("selected");
            ////    images += tag.ToString();
            ////}
            return new HtmlString(images);
        }
    }
}
