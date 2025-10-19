using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using SmartBreadcrumbs.Attributes;
using SmartBreadcrumbs.Nodes;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using SnitzCore.Service;

namespace MVCForum.Controllers
{
    public class FAQController : SnitzBaseController
    {
        private readonly IXmlFaqService _faqService;
        public FAQController(IMember memberService, ISnitzConfig config, IHtmlLocalizerFactory localizerFactory, 
            SnitzDbContext dbContext, IHttpContextAccessor httpContextAccessor,
            IXmlFaqService faqService) : base(memberService, config, localizerFactory, dbContext, httpContextAccessor)
        {
            _faqService = faqService;

        }

        // GET: FAQController
        [Breadcrumb("FAQ",FromAction = "Index",FromController = typeof(HomeController))]
        public IActionResult Index()
        {

            var vm = _faqService.GetCategoriesAsync().Result;
            return View(vm);
        }

        // GET: FAQController/Questions/5
        public IActionResult Questions(int id)
        {
            var vm = _faqService.GetQuestionsAsync(id).Result;
            return PartialView(vm);
        }

        // GET: FAQController/Create
        public IActionResult Create(int id)
        {
            var vm = new FaqQuestion
            {
                Id = _faqService.GetNextQuestionId(),
                Category = id
            };

            return PartialView("Edit",vm);
        }

        // POST: FAQController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(FaqQuestion question)
        {
            if(ModelState.IsValid)
            {
                _faqService.AddQuestionAsync(question).Wait();
            }
            return PartialView("Edit",question);
        }
        public IActionResult Category(int id)
        {
            if (id == -1)
            {
                var newCat = new FaqCategory(){Id = _faqService.GetNextCategoryId(),Order = 99};
                return PartialView(newCat);
            }
            var vm = _faqService.GetCategoryAsync(id).Result;
            return PartialView(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Category(FaqCategory category)
        {
            if(ModelState.IsValid)
            {
                _faqService.UpdateCategoryAsync(category).Wait();
            }
            return PartialView(category);
        }

        // GET: FAQController/Edit/5
        public IActionResult Edit(int id)
        {
            var vm = _faqService.GetQuestionAsync(id).Result;
            return PartialView(vm);
        }

        // POST: FAQController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FaqQuestion question)
        {
            if(ModelState.IsValid)
            {
                _faqService.UpdateQuestionAsync(question).Wait();
            }
            return PartialView(question);
        }

        // GET: FAQController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: FAQController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
