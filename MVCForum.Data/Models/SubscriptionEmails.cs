namespace SnitzCore.Data.Models
{
    public class SubscriptionEmail
    {
        public required string To { get; set; }
        public string? Sender { get; set; }
        public required string UserName { get; set; }
        public required string Subject { get; set; }

        public string? Unsubscribe { get; set; }
        public string? Topiclink { get; set; }
        public string? ForumTitle { get; set; }
        public string? Author { get; set; }

        public string? PostedIn { get; set; }
        public string? PostedInName { get; set; }


    }
    public class TopicSubscriptionEmail
    {
        public string? To { get; set; }
        public string? Sender { get; set; }
        public string? UserName { get; set; }
        public string? Subject { get; set; }

        public string? Unsubscribe { get; set; }
        public string? Topiclink { get; set; }
        public string? ForumTitle { get; set; }
        public string? Author { get; set; }

        public string? PostedIn { get; set; }
        public string? PostedInName { get; set; }

    }

}
