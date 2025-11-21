using System;

namespace SnitzCore.Data.Models
{
    public class VisitorLog
    {
        public int Id { get; set; }
        public DateTime VisitTime { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Path { get; set; }

        public string? UserName { get; set; }
    }
}
