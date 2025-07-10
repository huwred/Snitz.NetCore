using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace MVCForum.ViewModels
{
public class UploadViewModel
{
    [Required(ErrorMessage = "Please select a file")]
    public IFormFile AlbumImage { get; set; }

    public string? AllowedTypes { get; internal set; }

    public int MaxSize { get; internal set; }
}
}
