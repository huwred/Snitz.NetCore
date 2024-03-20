using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System.Threading.Tasks;
using BbCodeFormatter;
using Microsoft.AspNetCore.Hosting;

namespace SnitzCore.Service
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfiguration _emailConfig;
        private readonly ICodeProcessor _bbCodeProcessor;
        private readonly ISnitzConfig _config;
        private readonly IWebHostEnvironment _env;
        public EmailSender(EmailConfiguration emailConfig,ICodeProcessor bbCodeProcessor,IWebHostEnvironment env,ISnitzConfig config)
        {
            _emailConfig = emailConfig;
            _bbCodeProcessor = bbCodeProcessor;
            _env = env;
            _config = config;
        }

        public void ModerationEmail(Member? author, string subject, string message, Forum forum, dynamic post)
        {
            EmailMessage emessage = new EmailMessage(new List<string>(){author.Email}, subject, message);

            var emailMessage = CreateEmailMessage(emessage);
            Send(emailMessage);

        }

        public Task SendEmailAsync(EmailMessage message)
        {
            var emailMessage = CreateEmailMessage(message);
            return Send(emailMessage);
        }

        public void SendToFreind(dynamic model)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("email", model.FromEmail));
            emailMessage.To.Add(new MailboxAddress(model.ToName, model.ToEmail));
            emailMessage.Subject = model.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = _bbCodeProcessor.Format(model.Message) };
            Send(emailMessage);
        }

        public string ParseTemplate(string template,string subject, string email,string username, string callbackUrl, CultureInfo? culture, string? Extras = null)
        {
            if (culture != null)
            {
                template = culture.Name + Path.DirectorySeparatorChar + template;
            }
            var pathToFile = _env.WebRootPath  
                             + Path.DirectorySeparatorChar  
                             + "Templates"  
                             + Path.DirectorySeparatorChar  
                             + template;
            var builder = new BodyBuilder();
            using (StreamReader sourceReader = System.IO.File.OpenText(pathToFile))
            {

                builder.HtmlBody = sourceReader.ReadToEnd();

            }

            string messageBody = builder.HtmlBody
                .Replace("[SUBJECT]",subject)
                .Replace("[DATE]",$"{DateTime.Now:dddd, d MMMM yyyy}")
                .Replace("[EMAIL]",email)
                .Replace("[USER]",username)
                .Replace("[SERVER]",_config.ForumUrl)
                .Replace("[FORUM]",_config.ForumTitle)
                .Replace("[URL]",callbackUrl);
            if (Extras != null)
            {
                messageBody = messageBody.Replace("[EXTRATEXT]", Extras);
            }
            else
            {
                messageBody = messageBody.Replace("[EXTRATEXT]", "");
            }
            return messageBody;
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

        public string ParseSubscriptionTemplate(string template, string subject, string email, string username, string callbackUrl, CultureInfo? culture)
        {
            throw new NotImplementedException();
        }
    }
}
