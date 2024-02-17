using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Snitz.PhotoAlbum.Models;

namespace Snitz.PhotoAlbum.ViewModels
{

    public class UploadViewModel
    {
        public int MemberId { get; set; }
        public string Description { get; set; }
        public string ScientificName { get; set; }
        public string CommonName { get; set; }

        public int Group { get; set; }
        public SelectList GroupList { get; set; }
        public bool MarkAsPrivate { get; set; }
        public bool MarkAsDoNotFeature { get; set; }
    }

    public class SpeciesAlbum
    {
        [JsonIgnore]
        public List<global::Snitz.PhotoAlbum.Models.AlbumImage> Images { get; set; }
        public int GroupFilter { get; set; }

        public bool SrchUser { get; set; }
        public bool SrchSciName { get; set; }
        public bool SrchLocName { get; set; }
        public bool SrchDesc { get; set; }
        public bool SrchId { get; set; }
        public string SrchTerm { get; set; }

        public bool Thumbs { get; set; }
        public bool SpeciesOnly { get; set; }
        public string SortBy { get; set; }
        public string SortDesc { get; set; }

        [JsonIgnore]
        public SelectList GroupList { get; set; }
    }

    public class AdminAlbumViewModel
    {
        public bool PublicAlbum { get; set; }
        
        public List<AlbumGroup> Groups { get; set; }
        public bool MemberAlbums { get; set; }
        public bool FeaturedPhoto { get; set; }
        public bool Protected { get; set; }
    }
}