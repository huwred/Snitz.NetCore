using SnitzCore.Data.Extensions;

namespace Snitz.Events.Models
{
    public class BirthdayEventItem
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public bool IsAllDayEvent { get { return true; } }
        public string Title { get; set; }
        public DateTime? StartDate
        {
            get
            {
                return Dob.FromForumDateStr();
            }
            set
            {
                Dob = value.HasValue ? value.Value.ToForumDateStr(true) : null;
            }
        }

        public string Dob { get; set; }

    }
}