using Newtonsoft.Json;

namespace Snitz.Events.Models
{

    public class PublicHoliday
    {
        [JsonProperty(PropertyName = "localname")]
        public string Localname { get; set; }
        [JsonProperty(PropertyName = "date")]
        public EnricoDate Date { get; set; }
        [JsonProperty(PropertyName = "englishname")]
        public string Englishname { get; set; }

    }

    public class EnricoDate
    {
        [JsonProperty(PropertyName = "day")]
        public int Day { get; set; }
        [JsonProperty(PropertyName = "month")]
        public int Month { get; set; }
        [JsonProperty(PropertyName = "year")]
        public int Year { get; set; }
        [JsonProperty(PropertyName = "dayOfWeek")]
        public int DayOfWeek { get; set; }
    }

    public class EnricoCountry
    {
        [JsonProperty(PropertyName = "fullName")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "countryCode")]
        public string CountryCode { get; set; }
        [JsonProperty(PropertyName = "fromDate")]
        public EnricoDate FromDate { get; set; }
        [JsonProperty(PropertyName = "toDate")]
        public EnricoDate ToDate { get; set; }

        [JsonProperty(PropertyName = "regions")]
        public string[] Regions { get; set; }
    }
}