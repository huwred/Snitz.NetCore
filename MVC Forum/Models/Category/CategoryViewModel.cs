using SnitzCore.Data.Models;

namespace MVCForum.Models.Category
{
    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public Status Status { get; set; }
        public CategorySubscription Subscription { get; set; }
        public ModerationLevel Moderation { get; set; }
        public int Sort { get; set; }
    }
}
