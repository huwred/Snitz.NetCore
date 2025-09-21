using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnitzCore.Data.ViewModels
{
    public class RecentMembers
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? LastActivity { get; set; }

        public string? Avatar { get; set; }

    }
}
