using SnitzCore.Data.Models;
using System.Globalization;
using System.Threading.Tasks;


namespace SnitzCore.Data.Interfaces
{
    public interface IEmailSender
    {
        void ModerationEmail(Member? author, string subject, string message, Forum forum, dynamic post);
        Task SendEmailAsync(EmailMessage message);
        void SendToFreind(dynamic model);

        string ParseTemplate(string template, string subject, string email, string username, string callbackUrl,
            CultureInfo? culture, string? Extras = null);
    }
}
