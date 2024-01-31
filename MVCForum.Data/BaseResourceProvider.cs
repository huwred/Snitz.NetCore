using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SnitzCore.Data;

public abstract class BaseResourceProvider: ILanguageResource
{
    // Cache list of resources
    private static Dictionary<string, LanguageResource>? _resources;
    private static readonly object LockResources = new();

    protected BaseResourceProvider() {
        Cache = true; // By default, enable caching for performance
    }

    protected bool Cache { get; set; } // Cache resources ?
    //protected Dictionary<string, LanguageResource>? Resources { get { return _resources; } set { _resources = value; } } // Cache resources ?

    public void Reset()
    {
        new InMemoryCache().Remove("language.strings");
        _resources = null;
    }

    public string? GetLocalisedString(string name, string set, string? defvalue = "")
    {
        return GetResourceEntry(name,CultureInfo.CurrentCulture.TwoLetterISOLanguageName,set)?.Value ?? defvalue;
    }

    /// <summary>
    /// Returns a single resource for a specific culture
    /// </summary>
    /// <param name="name">Resorce name (ie key)</param>
    /// <param name="culture">Culture code</param>
    /// <returns>Resource</returns>
    public object GetResource(string name, string culture)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Resource name cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(culture))
            throw new ArgumentException("Culture name cannot be null or empty.");

        // normalize
        culture = culture.ToLowerInvariant();

        if (Cache && _resources == null) {
            // Fetch all resources
                
            lock (LockResources)
            {
                _resources ??= new InMemoryCache(60).GetOrSet("language.strings",
                    () => ReadResources().ToDictionary(r => $"{r.Culture.ToLowerInvariant()}.{r.Name}"));
            }
        }

        if (Cache) {
            if (_resources != null && _resources.TryGetValue($"{culture}.{name}", out LanguageResource? res))
            {
                return res.Value;
            }
            return "!*" + $"{culture}.{name}" + "*!";
        }

        return ReadResource(name, culture).Value;

    }

    /// <summary>
    /// Returns a single resource for a specific culture
    /// </summary>
    /// <param name="name">Resorce name (ie key)</param>
    /// <param name="culture">Culture code</param>
    /// <param name="resourceset"></param>
    /// <returns>Resource</returns>
    public object GetResource(string name, string culture, string resourceset)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Resource name cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(culture))
            throw new ArgumentException("Culture name cannot be null or empty.");

        // normalize
        culture = culture.ToLowerInvariant();

        if (Cache && _resources == null)
        {
            // Fetch all resources
            lock (LockResources)
            {
                _resources ??= new InMemoryCache(60).GetOrSet("language.strings",
                    () => ReadResources().ToDictionary(r => $"{r.Culture.ToLowerInvariant()}.{r.Name}"));
            }
        }

        if (Cache)
        {
            if (_resources != null && _resources.TryGetValue($"{culture}.{name}", out LanguageResource? res))
            {
                if(!String.IsNullOrWhiteSpace(res.Value))
                    return res.Value;
            }
            return "!*" + $"{culture}_{resourceset}.{name}" + "*!";

        }

        return ReadResource(name, culture).Value;

    }

    public LanguageResource GetResourceEntry(string name, string culture, string resourceset)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Resource name cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(culture))
            throw new ArgumentException("Culture name cannot be null or empty.");

        // normalize
        culture = culture.ToLowerInvariant();


        return ReadResource(name, culture);

    }
    public object GetResourceKey(string name, string culture)
    {
        return GetKey(name,culture);
    }


    public void AddResource(string name, string value)
    {
        AddResource(new LanguageResource(){Name = name,Value = value,Culture = "en"});
        new InMemoryCache().Remove("language.strings");
            
    }

    public void AddResource(string name, string value, string culture)
    {
        AddResource(new LanguageResource() { Name = name, Value = value, Culture = culture });
        new InMemoryCache().Remove("language.strings");
    }
    public void AddResource(string name, string value, string culture,string resourceset)
    {
        AddResource(new LanguageResource() { Name = name, Value = value, Culture = culture,ResourceSet = resourceset});
        new InMemoryCache().Remove("language.strings");
    }

    public void UpdateResource(int id, string name, string value, string culture, string resourceset)
    {
        UpdateResource(new LanguageResource() { Id=id, Name = name, Value = value, Culture = culture, ResourceSet = resourceset });
        new InMemoryCache().Remove("language.strings");
    }

    public void DeleteResource(int id)
    {
        DeleteResourceValue(id);
        new InMemoryCache().Remove("language.strings");
    }

    public void DeleteResource(string set, string name)
    {
        DeleteResources(set,name);
        new InMemoryCache().Remove("language.strings");
    }
    public void RenameResource(string old, string name,string set)
    {
        RenameResources(name, old,set);
        new InMemoryCache().Remove("language.strings");
    }
    public List<string> GetResourceSets()
    {
        return new List<string>(ReadResources().Select(r => r.ResourceSet).Distinct());

    }

    public List<LanguageResource> ReadAllResources(string resourceset)
    {
        return new List<LanguageResource>(ReadResources(resourceset));
    }

    public List<LanguageResource> ReadLangResources(string culture)
    {
        return new List<LanguageResource>(ReadLanguageResources(culture));
    }


    public IEnumerable<IGrouping<string,LanguageResource>> ReadAllResources()
    {
        var test = ReadResources().GroupBy(c => c.Name);
        return test;
    }

    public abstract string GetCulture(string langName);
    public abstract IEnumerable<string> GetCultures();
    protected abstract string? GetKey(string value, string culture);
    protected abstract IEnumerable<LanguageResource> ReadLanguageResources(string culture);

    /// <summary>
    /// Returns all resources for all cultures. (Needed for caching)
    /// </summary>
    /// <returns>A list of resources</returns>
    protected abstract IEnumerable<LanguageResource> ReadResources();

    /// <summary>
    /// Returns all resources for all cultures. (Needed for caching)
    /// </summary>
    /// <returns>A list of resources</returns>
    protected abstract IEnumerable<LanguageResource> ReadResources(string resourceset);

    /// <summary>
    /// Returns a single resource for a specific culture
    /// </summary>
    /// <param name="name">Resorce name (ie key)</param>
    /// <param name="culture">Culture code</param>
    /// <returns>Resource</returns>
    protected abstract LanguageResource? ReadResource(string name, string culture);

    /// <summary>
    /// Add a new resource string
    /// </summary>
    /// <param name="resource"></param>
    protected abstract Task AddResource(LanguageResource resource);
    protected abstract Task DeleteResources(string set, string name);
    protected abstract Task DeleteResourceValue(int id);

    /// <summary>
    /// Update a resource
    /// </summary>
    /// <param name="resource"></param>
    protected abstract Task UpdateResource(LanguageResource resource);

    protected abstract Task RenameResources(string newname, string oldname, string set);

}