using SnitzCore.Data.Models;

namespace SnitzCore.BackOffice.ViewModels
{
    public class LanguageViewModel
    {
        public List<LanguageResource>? DefaultStrings { get; set; }
        public List<string>? Languages { get; set; }
        public List<KeyValuePair<string,List<LanguageResource>>>? LanguageStrings { get; set; }

        public string? ResourceSet { get; set; }
    }
}
