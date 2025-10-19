namespace Snitz.PhotoAlbum.Models
{
    public class ImageMeta
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public double FileSizeKB { get; set; }
        public string Format { get; internal set; }
        public long FileSize { get; internal set; }
    }
}
