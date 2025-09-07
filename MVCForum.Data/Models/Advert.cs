using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SnitzCore.Data.Models
{
    [Serializable()]
    public class Advert
    {
        [XmlElement("ID")]
        public Guid Id { get; set; }
        [XmlElement("Impressions")]
        public int Impressions { get; set; }
        [XmlElement("Width")]
        public int Width { get; set; }
        [XmlElement("Height")]
        public int Height { get; set; }

        [XmlElement("ImageUrl")]
        public string Image { get; set; }
        [Required]
        [XmlElement("NavigateUrl")]
        public string Url { get; set; }
        [Required]
        [XmlElement("AlternateText")]
        public string AltText { get; set; }
        [Required]
        [XmlElement("Keyword")]
        public string Keyword { get; set; }
        [XmlElement("Weight")]
        public int Weight { get; set; }
        [XmlElement("Clicks")]
        public int Clicks { get; set; }

        //public IFormFile fileInput { get; set; }
        public Advert()
        {
            Clicks = 0;
            Impressions = 0;
            Weight = 0;
            Keyword = "side";
        }
    }
    [Serializable()]
    [System.Xml.Serialization.XmlRoot("AdCollection")]
    public class AdCollection
    {
        [XmlArray("Advertisements")]
        [XmlArrayItem("Advert", typeof(Advert))]
        public Advert[] Adverts { get; set; }
    }
}
