using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Snitz.PhotoAlbum.Models;

namespace Snitz.PhotoAlbum.ViewModels
{
    public class AlbumUploadViewModel
    {
        public string? Description { set;get; }
        public bool ShowCaption { set;get; }
        public bool Private { get; set; }
        public bool NotFeatured { get; set; }
        [Required(ErrorMessage = "Please select a file")]
        public IFormFile AlbumImage { set; get; }

        public int? Group { get; set; }
        public SelectList? GroupList { get; set; }
        public string? ScientificName { get; set; }
        public string? CommonName { get; set; }
        public string? AllowedTypes { get; internal set; }
        public int MaxSize { get; internal set; }

        public AlbumImage? Image { set; get; }
        public int Display { get; set; }
    }
}
