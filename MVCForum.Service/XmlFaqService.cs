using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SnitzCore.Service
{
    public class FaqCategory
    {
        public int Id { get; set; }
        [Required]
        [StringLength(200)]
        public string Description { get; set; }
        [StringLength(100)]
        public string? Roles { get; set; }
        [Range(0, 99)]
        public int Order {get;set;}
    }

    public class FaqQuestion
    {
        public int Id { get; set; }
        [Required]
        [StringLength(200)]
        public string Title { get; set; }
        [Range(0, 99)]
        public int Order { get; set; }
        [Required]
        public int Category { get; set; }
        [Required]
        [StringLength(4000)]
        public string Answer { get; set; }
    }

    public interface IXmlFaqService
    {
        Task<List<FaqCategory>> GetCategoriesAsync();
        Task<FaqCategory> GetCategoryAsync(int id);
        Task<List<FaqQuestion>> GetQuestionsAsync(int catid);
        Task<FaqQuestion> GetQuestionAsync(int id);
        Task AddCategoryAsync(FaqCategory category);
        Task UpdateCategoryAsync(FaqCategory category);
        Task DeleteCategoryAsync(int id);
        Task AddQuestionAsync(FaqQuestion question);
        Task UpdateQuestionAsync(FaqQuestion question);
        Task DeleteQuestionAsync(int id);
        int GetNextQuestionId();
        int GetNextCategoryId();
    }

    public class XmlFaqService : IXmlFaqService
    {
        private readonly string _filePath;

        public XmlFaqService(string filePath)
        {
            _filePath = filePath;
        }

        public Task<List<FaqCategory>> GetCategoriesAsync()
        {
            var doc = LoadDocument();
            var categories = doc.Descendants("Category")
                .Select(x => new FaqCategory
                {
                    Id = (int)x.Attribute("id"),
                    Description = (string)x.Attribute("Description"),
                    Roles = (string)x.Attribute("Roles"),
                    Order = (int)x.Attribute("Order")
                }).OrderBy(c=>c.Order).ToList();
            return Task.FromResult(categories);
        }

        public Task<List<FaqQuestion>> GetQuestionsAsync(int catid)
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
                }).Where(q=>q.Category == catid).OrderBy(q=>q.Order).ToList();
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
                doc.Descendants("Question").Where(q=>(int)q.Attribute("category") == id).Remove();
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
            else
            {
                return AddQuestionAsync(question);
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

        public Task<FaqQuestion> GetQuestionAsync(int id)
        {
            var doc = LoadDocument();
            var question = doc.Descendants("Question")
                .Select(x => new FaqQuestion
                {
                    Id = (int)x.Attribute("id"),
                    Title = (string)x.Attribute("title"),
                    Order = (int?)x.Attribute("order") ?? 0,
                    Category = (int)x.Attribute("category"),
                    Answer = (string)x.Element("Answer")
                }).Where(q=>q.Id == id).First();

            return Task.FromResult(question);
        }

        /// <summary>
        /// Generates the next available question ID by finding the highest existing ID in the document and incrementing
        /// it.
        /// </summary>
        /// <remarks>This method loads the document, identifies all question elements, and determines the
        /// maximum value of their "id" attributes. If no questions are present, the method returns 1 as the starting
        /// ID.</remarks>
        /// <returns>The next available question ID as an integer. Returns 1 if no questions exist in the document.</returns>
        public int GetNextQuestionId()
        {
            var doc = LoadDocument();
            var maxId = doc.Descendants("Question")
                           .Select(x => (int?)x.Attribute("id") ?? 0)
                           .DefaultIfEmpty(0)
                           .Max();
            return maxId + 1;
        }
        public int GetNextCategoryId()
        {
            var doc = LoadDocument();
            var maxId = doc.Descendants("Category")
                           .Select(x => (int?)x.Attribute("id") ?? 0)
                           .DefaultIfEmpty(0)
                           .Max();
            return maxId + 1;
        }
        /// <summary>
        /// Loads an XML document based on the current UI culture's language.
        /// </summary>
        /// <remarks>The method constructs the file path using the current UI culture's two-letter ISO
        /// language name and loads the corresponding XML document. Ensure that the file exists at the specified
        /// location to avoid exceptions.</remarks>
        /// <returns>An <see cref="XDocument"/> representing the loaded XML document.</returns>
        private XDocument LoadDocument()
        {
            CultureInfo cultureInfo = CultureInfo.CurrentUICulture;
            var lang = cultureInfo.TwoLetterISOLanguageName;
            var filePath = Path.Combine(_filePath,$"faq.category.{lang}.xml");

            return XDocument.Load(filePath);
        }

        /// <summary>
        /// Saves the specified XML document to a file in the application's language-specific directory.
        /// </summary>
        /// <remarks>The file is saved with a name that includes the current UI culture's two-letter ISO
        /// language code. The file is stored in the directory specified by the application's internal file
        /// path.</remarks>
        /// <param name="doc">The <see cref="XDocument"/> to save. Must not be <see langword="null"/>.</param>
        private void SaveDocument(XDocument doc)
        {
            CultureInfo cultureInfo = CultureInfo.CurrentUICulture;
            var lang = cultureInfo.TwoLetterISOLanguageName;
            var filePath = Path.Combine(_filePath,$"faq.category.{lang}.xml");
            doc.Save(filePath);
        }

        public Task<FaqCategory> GetCategoryAsync(int id)
        {
            var doc = LoadDocument();
            var category = doc.Descendants("Category")
                .Select(x => new FaqCategory
                {
                    Id = (int)x.Attribute("id"),
                    Description = (string)x.Attribute("Description"),
                    Roles = (string)x.Attribute("Roles")
                }).Single(c=>c.Id == id);

            return Task.FromResult(category);
        }
    }
}