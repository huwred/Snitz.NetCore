using Microsoft.EntityFrameworkCore;
using SnitzCore.Data.Extensions;
using SnitzCore.Service.Extensions;

namespace Snitz.Events.Models
{
    public class EventsRepository : IDisposable
    {
        private readonly EventContext _dbContext;

        public EventsRepository(EventContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Dispose()
        {

        }
        public CalendarEventItem? GetById(int id)
        {
            return _dbContext.EventItems.Include(e=>e.Topic).FirstOrDefault(e=>e.Id == id);
        }

        public CalendarEventItem? GetClubEventById(int id)
        {
            return _dbContext.EventItems.Include(e=>e.Author)
                .Include(ce => ce.Cat)
                .Include(ce => ce.Club)
                .Include(ce => ce.Loc)                
                .FirstOrDefault(e=>e.Id == id && e.ClubId != null);
        }
        public IEnumerable<CalendarEventItem> GetAllClubEvents(string catid, string old, string? start, string? end)
        {
            var clubevents = _dbContext.EventItems
                .Include(ce => ce.Author)
                .Include(ce => ce.Cat)
                .Include(ce => ce.Club)
                .Include(ce => ce.Loc)
                .Where(ce => ce.ClubId != null && ce.TopicId < 0)
                .ToList();

            if (start != null)
                start = start.ToEnglishNumber().Replace("T", "").Replace(":", "");
            if (end != null)
                end = end.ToEnglishNumber().Replace("T", "").Replace(":", "");

            if (Convert.ToInt32(catid) > 0)
            {
                clubevents = clubevents.Where(ce => ce.CatId == Convert.ToInt32(catid)).ToList();
            }
            else if (Convert.ToInt32(old) < 0)
            {
                start = null;
                end = DateTime.UtcNow.ToForumDateStr();
            }
            if (start != null && end != null)
            {
                clubevents = clubevents.Where(ce => string.Compare(ce.Start, start) >= 0
                                        && string.Compare(ce.Start, end) <= 0).ToList();
            }
            else if (start != null)
            {
                clubevents = clubevents.Where(ce => string.Compare(ce.Start, start) >= 0).ToList();
            }
            else if (end != null)
            {
                clubevents = clubevents.Where(ce=>string.Compare(ce.Start, end) <= 0).ToList();
            }

            return clubevents.OrderBy(ce => ce.Start).ToList();
        }


    }
}
