using Microsoft.Extensions.Logging;
using SnitzCore.Data;
using SnitzCore.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnitzCore.Service.Extensions
{

    public interface ILogRepository
    {
        Task InsertAsync(List<VisitorLog> logMessages);
    }
    public class VisitorLogRepository : ILogRepository
    {
        private readonly SnitzDbContext _context;

        public VisitorLogRepository(SnitzDbContext context)
        {
            _context = context;
        }

        public async Task InsertAsync(List<VisitorLog> logMessages)
        {
            await _context.VisitorLog.AddRangeAsync(logMessages);
            await _context.SaveChangesAsync();
        }
    }
}
