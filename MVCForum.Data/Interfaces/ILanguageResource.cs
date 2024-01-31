using SnitzCore.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace SnitzCore.Data.Interfaces
{
    public interface ILanguageResource
    {
        object GetResource(string name, string culture);
        object GetResource(string name, string culture, string resourceset);
        LanguageResource GetResourceEntry(string name, string culture, string resourceset);
        object GetResourceKey(string name, string culture);
        //object GetKey(string value, string culture);
        void AddResource(string name, string value);
        void AddResource(string name, string value, string culture);
        void AddResource(string name, string value, string culture, string resourceset);
        void UpdateResource(int id, string name, string value, string culture, string resourceset);
        void DeleteResource(int id);
        void DeleteResource(string set, string name);
        List<string> GetResourceSets();
        List<LanguageResource> ReadAllResources(string resourceset);
        List<LanguageResource> ReadLangResources(string culture);
        IEnumerable<IGrouping<string, LanguageResource>> ReadAllResources();
        string GetCulture(string langName);
        IEnumerable<string> GetCultures();

        void RenameResource(string old, string name, string set);
        void Reset();
        string? GetLocalisedString(string name, string set,string? defvalue = "");
    }
}
