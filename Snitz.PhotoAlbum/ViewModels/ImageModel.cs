using Snitz.PhotoAlbum.Models;

namespace Snitz.PhotoAlbum.ViewModels
{
    /// <summary>
    /// ViewModel for managing a collection of gallery images and the current image selection.
    /// </summary>
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

    /// <summary>
    /// Represents an image in the gallery with its metadata.
    /// </summary>
    public class GalleryImage
    {
        /// <summary>
        /// Gets or sets the unique identifier for the image.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the image.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the image.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the file path of the image.
        /// </summary>
        public string Path { get; set; }
    }
}