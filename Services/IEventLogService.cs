using System.Threading.Tasks;
using wpf_projekt.Models;

namespace wpf_projekt.Services
{
    public interface IEventLogService
    {
        Task LogAsync(EventType eventType, string description, int? entityId = null, int? userId = null);
    }
}