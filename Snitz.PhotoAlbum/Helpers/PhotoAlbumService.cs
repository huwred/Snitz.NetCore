using Snitz.PhotoAlbum.Models;
using SnitzCore.Data.Interfaces;

namespace Snitz.PhotoAlbum.Helpers
{
    public class PhotoAlbumService : ISnitzStartupService
    {

        public bool EnabledForTopic(int topicid)
        {
            throw new NotImplementedException();
        }

        public int AuthLevel(int forumid)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddItemAsync(AlbumImage item)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> Get(int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddItemAsync(object item)
        {
            throw new NotImplementedException();
        }
    }
}
