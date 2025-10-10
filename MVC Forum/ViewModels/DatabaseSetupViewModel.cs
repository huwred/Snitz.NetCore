using System.ComponentModel.DataAnnotations;

namespace MVCForum.ViewModels
{
    public class DatabaseSetupViewModel
    {
        [Required]
        public string Server { get; set; }

        [Required]
        public string Database { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }

        public bool UseIntegratedSecurity { get; set; }
        public bool UseEncryption { get; set; }
        public bool TrustServerCertificate { get; set; }

        [Required]
        public string Provider { get; set; }

        public string? TestResult { get; set; } 

        public string? LandingPage { get; set; }
        public string? Version {get;set;}

        public bool IsConfigured {get;set;}

        public string? ForumUrl {get;set;}

    }

}
