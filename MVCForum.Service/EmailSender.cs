using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using MailKit.Security;
using MimeKit;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System.Threading.Tasks;
using BbCodeFormatter;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Localization;
using System.Threading;
using System.Diagnostics;


namespace SnitzCore.Service
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfiguration _emailConfig;
        private readonly ICodeProcessor _bbCodeProcessor;
        private readonly ISnitzConfig _config;
        private readonly IWebHostEnvironment _env;
        private readonly LanguageService  _languageResource;
        public EmailSender(EmailConfiguration emailConfig,ICodeProcessor bbCodeProcessor,IWebHostEnvironment env,ISnitzConfig config,IHtmlLocalizerFactory localizerFactory)
        {
            _emailConfig = emailConfig;
            _bbCodeProcessor = bbCodeProcessor;
            _env = env;
            _config = config;
            _languageResource = (LanguageService)localizerFactory.Create("SnitzController", "MVCForum");
        }

        public void ModerationEmail(Member? author, string subject, string message, Forum forum, dynamic post)
        {
            EmailMessage emessage = new EmailMessage(new List<string>(){author?.Email!}, subject, message);

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

        public Task MoveNotify(Member author, Post topic)
        {
            if(author.Email == null){
                return Task.CompletedTask;
            }
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;;
            
            EmailMessage emessage = new EmailMessage(new List<string>(){author.Email!}, _languageResource.GetString("MoveNotify"), 
                ParseTemplate("movenotify.html",_languageResource.GetString("MoveNotify"),author.Email!,author.Name, $"{_config.ForumUrl}Topic/Index/{topic.Id}" , cultureInfo.Name));

            var emailMessage = CreateEmailMessage(emessage);
            return Send(emailMessage);
        }

        public string ParseSubscriptionTemplate(string template, string posttype, string postname, string authorname, string toname, string postUrl, string unsubUrl,
            string? lang)
        {
            var pathToFile = TemplateFile(template, lang);
            var builder = new BodyBuilder();
            using (StreamReader sourceReader = System.IO.File.OpenText(pathToFile))
            {

                builder.HtmlBody = sourceReader.ReadToEnd();

            }

            string messageBody = builder.HtmlBody
                .Replace("[DATE]",$"{DateTime.Now:dddd, d MMMM yyyy}")
                .Replace("[AUTHOR]",authorname)
                .Replace("[USER]",toname)
                .Replace("[SERVER]",_config.ForumUrl)
                .Replace("[FORUM]",_config.ForumTitle)
                .Replace("[POSTEDIN]",posttype)
                .Replace("[POSTEDNAME]",postname)
                .Replace("[URL]",postUrl)
                .Replace("[UNSUBSCRIBE]",unsubUrl);
            return messageBody;
        }

        public Task TopicMergeEmail(Post topic, Post mainTopic, Member author)
        {
            if (_config.GetIntValue("STREMAIL",0) != 1)
                return Task.CompletedTask;
            if(author.Email == null){
                return Task.CompletedTask;
            }
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;;
            
            EmailMessage emessage = new EmailMessage(new List<string>(){author.Email!}, _languageResource.GetString("MoveNotify"), 
                ParseTemplate("mergetopic.html",_languageResource.GetString("tipMergeTopic"),author.Email!,author.Name, $"{_config.ForumUrl}Topic/Index/{mainTopic.Id}" , cultureInfo.Name));

            var emailMessage = CreateEmailMessage(emessage);
            return Send(emailMessage);

        }

        public void SendPMNotification(Member taggedmember)
        {
            if(taggedmember.Email == null){
                return;
            }
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;;
            
            EmailMessage emessage = new EmailMessage(new List<string>(){taggedmember.Email}, _languageResource.GetString("NewMessage"), 
                ParseTemplate("pmnotify.html",_languageResource.GetString("NewMessage"),taggedmember.Email,taggedmember.Name, $"{_config.ForumUrl}PrivateMessage/Inbox" , cultureInfo.Name));

            var emailMessage = CreateEmailMessage(emessage);
            Send(emailMessage);
        }

        public string ParseTemplate(string template,string subject, string toemail,string tousername, string callbackUrl, string? lang, string? Extras = null)
        {
            var pathToFile = TemplateFile(template, lang);
            var builder = new BodyBuilder();
            using (StreamReader sourceReader = System.IO.File.OpenText(pathToFile))
            {

                builder.HtmlBody = sourceReader.ReadToEnd();

            }

            string messageBody = builder.HtmlBody
                .Replace("[SUBJECT]",subject)
                .Replace("[DATE]",$"{DateTime.Now:dddd, d MMMM yyyy}")
                .Replace("[EMAIL]",toemail)
                .Replace("[USER]",tousername)
                .Replace("[SERVER]",_config.ForumUrl)
                .Replace("[FORUM]",_config.ForumTitle)
                .Replace("[URL]",callbackUrl);
            messageBody = messageBody.Replace("[EXTRATEXT]", Extras ?? "");
            return messageBody;
        }

        private string TemplateFile(string template, string? lang)
        {
            var langtemplate = template;
            if (lang != null)
            {
                langtemplate = Path.Combine(lang,template);
            }
            var pathToFile = Path.Combine(_env.WebRootPath,"Templates",langtemplate);

            if (!File.Exists(pathToFile)) //fallback to english
            {
                pathToFile = Path.Combine(_env.WebRootPath,"Templates","en-GB",template);
            }

            return pathToFile;
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
            using (var client = new MailKit.Net.Smtp.SmtpClient())
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
                    var response = client.Send(mailMessage);
                    Debug.WriteLine(response);
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
