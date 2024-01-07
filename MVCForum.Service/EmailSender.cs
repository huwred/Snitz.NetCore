using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System.Threading.Tasks;

namespace SnitzCore.Service
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfiguration _emailConfig;
        public EmailSender(EmailConfiguration emailConfig)
        {
            _emailConfig = emailConfig;
        }

        public Task SendEmailAsync(EmailMessage message)
        {
            var emailMessage = CreateEmailMessage(message);
            return Send(emailMessage);
        }

        private MimeMessage CreateEmailMessage(EmailMessage message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("email", _emailConfig.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = message.Content };
            return emailMessage;
        }
        private Task Send(MimeMessage mailMessage)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    if (_emailConfig.Port == 587)
                    {
                        client.Connect (_emailConfig.SmtpServer, _emailConfig.Port, SecureSocketOptions.StartTls);
                    }
                    else
                    {
                        client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, _emailConfig.Port != 25);
                    }

                    
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    if (_emailConfig.RequireLogin)
                    {
                        client.Authenticate(_emailConfig.UserName, _emailConfig.Password);
                    }
                    client.Send(mailMessage);
                }
                catch
                {
                    //log an error message or throw an exception or both.
                    throw;
                }
                finally
                {
                    client.Disconnect(true);
                    client.Dispose();
                    
                }
            }
            return Task.CompletedTask;
        }
    }
}
