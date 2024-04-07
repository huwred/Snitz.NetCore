using Snitz.PhotoAlbum.Models;

namespace Snitz.PhotoAlbum.ViewModels
{
    public class ImageModel
    {
        List<GalleryImage> _images = new();

        public ImageModel()
        {
            _images = new List<GalleryImage>();
        }

        public List<GalleryImage> Images
        {
            get { return _images; }
            set { _images = value; }
        }
        public AlbumImage CurrentImage { get; set; }
        public int CurrentIdx { get; set; }
    }

    public class GalleryImage
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
    }
}