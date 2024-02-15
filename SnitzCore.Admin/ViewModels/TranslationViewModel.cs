using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SnitzCore.Data.Models;

namespace SnitzCore.BackOffice.ViewModels
{
    public class TranslationViewModel
    {
        public string? filterby { get; set; }
        public string? filter { get; set; }
        public List<LanguageResource> Resources { get; set; } = null!;

        public List<string?> ResourceSets { get; set; }
    }
}
