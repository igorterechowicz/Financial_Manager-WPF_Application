using System.Threading.Tasks;
using wpf_projekt.Models;
using wpf_projekt.models;

namespace wpf_projekt.Services
{
    public interface IEventLogService
    {
        Task LogAsync(EventType eventType, string description, int? entityId = null, int? userId = null);
    }
}