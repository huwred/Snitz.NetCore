
using SnitzCore.Data.Models;

namespace SnitzCore.BackOffice.ViewModels
{
    public class TranslationViewModel
    {
        public string? filterby { get; set; }
        public string? filter { get; set; }
        public List<LanguageResource> Resources { get; set; } = null!;

        public List<string?> ResourceSets { get; set; }

        public List<KeyValuePair<string, List<LanguageResource>>> Translations{get;set;}
    }
}
