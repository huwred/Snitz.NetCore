namespace SnitzCore.BackOffice.ViewModels
{
    public class TemplateViewModel
    {
        public List<string>? Templates { get; set; }

        public required string TemplateLang { get; set; }
        public required string TemplateHtml { get; set; }
        public required string TemplateFile { get; set; }

    }
}
