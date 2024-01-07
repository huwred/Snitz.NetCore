using Microsoft.IdentityModel.Tokens;
using SnitzCore.Data;
using SnitzCore.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnitzCore.Service
{
    public class LanguageService : BaseResourceProvider
    {
    

        private readonly SnitzDbContext _dbContext;
        public LanguageService(SnitzDbContext dbContext){

            _dbContext = dbContext;

        }

        public override string GetCulture(string langName)
        {
            var lang = _dbContext.LanguageResources.Where(l => l.Culture == langName);
            if (lang.Any())
            {
                return langName;
            }

            return "en";
        }

        public override IEnumerable<string> GetCultures()
        {
            return _dbContext.LanguageResources.Select(l => l.Culture).Distinct();

        }

        protected override string? GetKey(string value, string culture)
        {
            var langres = _dbContext.LanguageResources.SingleOrDefault(l => l.Culture == culture && l.Name == value);
            return langres?.Value;

        }

        protected override IEnumerable<LanguageResource> ReadResources()
        {
            return _dbContext.LanguageResources.OrderBy(l => l.Culture).ThenBy(l => l.Name);
        }
        protected override IEnumerable<LanguageResource> ReadLanguageResources(string culture)
        {
            return _dbContext.LanguageResources.Where(l=>l.Culture==culture).OrderBy(l => l.Name);
        }
        protected override IEnumerable<LanguageResource> ReadResources(string resourceset)
        {
            if (resourceset.IsNullOrEmpty())
            {
                return _dbContext.LanguageResources.Where(l=>l.ResourceSet==null);
            }

            return _dbContext.LanguageResources.Where(l=>l.ResourceSet==resourceset);
        }

        protected override LanguageResource? ReadResource(string name, string culture)
        {
            return _dbContext.LanguageResources.SingleOrDefault(l => l.Culture == culture && l.Name == name);

        }

        protected override async Task AddResource(LanguageResource resource)
        {
            _dbContext.LanguageResources.Add(resource);
            await _dbContext.SaveChangesAsync();
        }

        protected override async Task DeleteResources(string set, string name)
        {
            var langres = _dbContext.LanguageResources.SingleOrDefault(l => l.ResourceSet == set && l.Name == name);
            if (langres != null) _dbContext.LanguageResources.Remove(langres);
            await _dbContext.SaveChangesAsync();

        }

        protected override async Task DeleteResourceValue(int id)
        {
            var langres = _dbContext.LanguageResources.SingleOrDefault(l => l.Id == id);
            if (langres != null) _dbContext.LanguageResources.Remove(langres);
            await _dbContext.SaveChangesAsync();
        }

        protected override async Task UpdateResource(LanguageResource resource)
        {
            _dbContext.LanguageResources.Update(resource);
            await _dbContext.SaveChangesAsync();

            
        }

        protected override async Task RenameResources(string newname, string oldname, string set)
        {
            var langres = _dbContext.LanguageResources.SingleOrDefault(l => l.ResourceSet == set && l.Name==oldname);
            if (langres != null)
            {
                langres.Name = newname;
                _dbContext.LanguageResources.Update(langres);
            }

            await _dbContext.SaveChangesAsync();

        }
    }
}
