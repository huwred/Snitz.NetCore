using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace SnitzCore.Service
{
    public class EmoticonService : IEmoticon
    {
        private readonly EmoticonList? _objects;
        public EmoticonService()
        {
            var path = System.IO.Path.Combine("App_Data", "emoticons.xml");
            path = path.Replace("\\", System.IO.Path.DirectorySeparatorChar.ToString());

            _objects = XmlExtensions.DeserializeListFromXml<EmoticonList>(path);
        }

        public Emoticon? GetByName(string name)
        {
            if (_objects?.Emoticons != null) return _objects?.Emoticons.SingleOrDefault(e => e.Name == name);
            return null;
        }

        public IEnumerable<Emoticon> GetAll()
        {
            if (_objects is { Emoticons: { } }) return _objects.Emoticons.DistinctBy(e => e.Name);
            return Enumerable.Empty<Emoticon>();
        }
    }
}
