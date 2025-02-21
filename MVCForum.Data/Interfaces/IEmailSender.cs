using SnitzCore.Data.Models;
using System.Threading.Tasks;


namespace SnitzCore.Data.Interfaces
{
    public interface IEmailSender
    {
        void ModerationEmail(Member? author, string subject, string message, Forum forum, dynamic post);
        Task SendEmailAsync(EmailMessage message);
        void SendToFreind(dynamic model);
        Task MoveNotify(Member author, Post topic);

        string ParseTemplate(string template, string subject, string toemail, string tousername, string callbackUrl,
            string? lang, string? Extras = null);

        string ParseSubscriptionTemplate(string template, string posttype, string postname, string authorname,string toname, string postUrl, string unsubUrl,
            string? lang);
        Task TopicMergeEmail(Post topic, Post mainTopic, Member author);
        void SendPMNotification(Member taggedmember);
    }
}
