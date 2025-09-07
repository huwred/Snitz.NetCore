using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnitzCore.Service.Extensions
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;

    public class XmlHelper<T>
    {
        private readonly string _filePath;

        public XmlHelper(string filePath)
        {
            _filePath = filePath;
        }

        public T? Read()
        {
            if (!File.Exists(_filePath)) return default;

            using var stream = new FileStream(_filePath, FileMode.Open);
            var serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(stream) ?? default;
        }

        public void Write(T data)
        {
            using var stream = new FileStream(_filePath, FileMode.Create);
            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(stream, data);
        }
    }
}
