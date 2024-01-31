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
            _objects = XmlExtensions.DeserializeListFromXml<EmoticonList>("App_Data\\emoticons.xml");
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
