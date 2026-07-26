using TalentHub.Domain.Common;
using TalentHub.Domain.Enums.Notification;

namespace TalentHub.Domain.Entities
{
    public class Notification :BaseEntity 
    {
        public string UserId { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string Body { get; set; } = null!;

        public NotificationType Type { get; set; }

        public bool IsRead { get; set; }

        public DateTime? ReadAt { get; set; }

        public int? ReferenceId { get; set; }

        // Navigation Properties
    }
}
