using SnitzCore.Data.Models;
using System.Threading.Tasks;


namespace SnitzCore.Data.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(EmailMessage message);
    }
}
