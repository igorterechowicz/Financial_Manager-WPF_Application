using System;
using System.ComponentModel.DataAnnotations;

namespace wpf_projekt.Models
{
    public class EventLog
    {
        [Key]
        public int Id { get; set; }

        public DateTime Timestamp { get; set; }

        public int? UserId { get; set; }

        public EventType EventType { get; set; }

        public string Description { get; set; }

        public int? EntityId { get; set; }
    }
}