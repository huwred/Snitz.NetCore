

namespace SnitzCore.Data.Models
{
    public class EmailConfiguration
    {
        public string? From { get; set; }
        public string? SmtpServer { get; set; }
        public int Port { get; set; } = 25;

        public bool RequireLogin { get; set; } = false;
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }
}
