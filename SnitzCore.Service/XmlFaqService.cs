using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SnitzCore.Service
{
    public class FaqCategory
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Roles { get; set; }
    }

    public class FaqQuestion
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public int Category { get; set; }
        public string Answer { get; set; }
    }

    public interface IXmlFaqService
    {
        Task<List<FaqCategory>> GetCategoriesAsync();
        Task<List<FaqQuestion>> GetQuestionsAsync();
        Task AddCategoryAsync(FaqCategory category);
        Task UpdateCategoryAsync(FaqCategory category);
        Task DeleteCategoryAsync(int id);
        Task AddQuestionAsync(FaqQuestion question);
        Task UpdateQuestionAsync(FaqQuestion question);
        Task DeleteQuestionAsync(int id);
    }

    public class XmlFaqService : IXmlFaqService
    {
        private readonly string _filePath;

        public XmlFaqService(string filePath)
        {
            _filePath = filePath;
        }

        private XDocument LoadDocument()
        {
            return XDocument.Load(_filePath);
        }

        private void SaveDocument(XDocument doc)
        {
            doc.Save(_filePath);
        }

        public Task<List<FaqCategory>> GetCategoriesAsync()
        {
            var doc = LoadDocument();
            var categories = doc.Descendants("Category")
                .Select(x => new FaqCategory
                {
                    Id = (int)x.Attribute("id"),
                    Description = (string)x.Attribute("Description"),
                    Roles = (string)x.Attribute("Roles")
                }).ToList();
            return Task.FromResult(categories);
        }

        public Task<List<FaqQuestion>> GetQuestionsAsync()
        {
            var doc = LoadDocument();
            var questions = doc.Descendants("Question")
                .Select(x => new FaqQuestion
                {
                    Id = (int)x.Attribute("id"),
                    Title = (string)x.Attribute("title"),
                    Order = (int?)x.Attribute("order") ?? 0,
                    Category = (int)x.Attribute("category"),
                    Answer = (string)x.Element("Answer")
                }).ToList();
            return Task.FromResult(questions);
        }

        public Task AddCategoryAsync(FaqCategory category)
        {
            var doc = LoadDocument();
            var categories = doc.Descendants("Categories").First();
            categories.Add(new XElement("Category",
                new XAttribute("id", category.Id),
                new XAttribute("Description", category.Description),
                new XAttribute("Roles", category.Roles ?? "")
            ));
            SaveDocument(doc);
            return Task.CompletedTask;
        }

        public Task UpdateCategoryAsync(FaqCategory category)
        {
            var doc = LoadDocument();
            var cat = doc.Descendants("Category")
                .FirstOrDefault(x => (int)x.Attribute("id") == category.Id);
            if (cat != null)
            {
                cat.SetAttributeValue("Description", category.Description);
                cat.SetAttributeValue("Roles", category.Roles ?? "");
                SaveDocument(doc);
            }
            return Task.CompletedTask;
        }

        public Task DeleteCategoryAsync(int id)
        {
            var doc = LoadDocument();
            var cat = doc.Descendants("Category")
                .FirstOrDefault(x => (int)x.Attribute("id") == id);
            if (cat != null)
            {
                cat.Remove();
                SaveDocument(doc);
            }
            return Task.CompletedTask;
        }

        public Task AddQuestionAsync(FaqQuestion question)
        {
            var doc = LoadDocument();
            var questions = doc.Descendants("Questions").First();
            questions.Add(new XElement("Question",
                new XAttribute("id", question.Id),
                new XAttribute("title", question.Title),
                new XAttribute("order", question.Order),
                new XAttribute("category", question.Category),
                new XElement("Answer", new XCData(question.Answer ?? ""))
            ));
            SaveDocument(doc);
            return Task.CompletedTask;
        }

        public Task UpdateQuestionAsync(FaqQuestion question)
        {
            var doc = LoadDocument();
            var q = doc.Descendants("Question")
                .FirstOrDefault(x => (int)x.Attribute("id") == question.Id);
            if (q != null)
            {
                q.SetAttributeValue("title", question.Title);
                q.SetAttributeValue("order", question.Order);
                q.SetAttributeValue("category", question.Category);
                q.Element("Answer")?.Remove();
                q.Add(new XElement("Answer", new XCData(question.Answer ?? "")));
                SaveDocument(doc);
            }
            return Task.CompletedTask;
        }

        public Task DeleteQuestionAsync(int id)
        {
            var doc = LoadDocument();
            var q = doc.Descendants("Question")
                .FirstOrDefault(x => (int)x.Attribute("id") == id);
            if (q != null)
            {
                q.Remove();
                SaveDocument(doc);
            }
            return Task.CompletedTask;
        }
    }
}