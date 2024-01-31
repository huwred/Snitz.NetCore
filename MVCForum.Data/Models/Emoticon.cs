using System.Xml.Serialization;

namespace SnitzCore.Data.Models;

public class Emoticon
{
    [XmlAttribute("name")]
    public string? Name { get; set; }

    [XmlAttribute("code")]
    public string? Code { get; set; }

    [XmlAttribute("image")]
    public string? Image { get; set; }
}

[XmlRoot("emoticons")]
public class EmoticonList
{
    [XmlElement("emoticon")]
    public Emoticon[]? Emoticons { get; set; }
}