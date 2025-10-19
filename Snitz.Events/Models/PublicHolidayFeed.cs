namespace Snitz.Events.Models
{

    public class PublicHoliday
    {

        public string localName { get; set; }

        public EnricoDate date { get; set; }

        public string englishName { get; set; }

    }

    public class EnricoDate
    {

        public int day { get; set; }

        public int month { get; set; }

        public int year { get; set; }

        public int dayOfWeek { get; set; }
    }

    public class EnricoCountry
    {

        public string fullName { get; set; }

        public string countryCode { get; set; }

        public EnricoDate fromDate { get; set; }

        public EnricoDate toDate { get; set; }

        public string[] regions { get; set; }
    }
}