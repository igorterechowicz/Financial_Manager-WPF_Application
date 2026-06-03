using System;
using System.Threading.Tasks;
using wpf_projekt.Data;
using wpf_projekt.Models;

namespace wpf_projekt.Services
{
    public class EventLogService : IEventLogService
    {
        private readonly AppDbContext _context;

        public EventLogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(EventType eventType, string description, int? entityId = null, int? userId = null)
        {
            var log = new EventLog
            {
                Timestamp = DateTime.Now,
                EventType = eventType,
                Description = description,
                EntityId = entityId,
                UserId = userId
            };

            _context.EventLogs.Add(log);

            await _context.SaveChangesAsync();
        }
    }
}