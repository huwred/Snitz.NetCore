using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnitzCore.Data.Interfaces
{
    public interface ISnitzStartupService
    {
        bool EnabledForTopic(int topicid);
        int AuthLevel(int forumid);

        IEnumerable<object> Get(int id);
        Task<bool> AddItemAsync(object item);
    }
}
