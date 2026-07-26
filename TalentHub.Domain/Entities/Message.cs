using TalentHub.Domain.Common;

namespace TalentHub.Domain.Entities
{
    public class Message :BaseEntity
    {
        public int ConversationId { get; set; }

        public string SenderId { get; set; } = null!;

        public string Content { get; set; } = null!;

        public bool IsRead { get; set; }

        public DateTime? ReadAt { get; set; }

        // Navigation Properties
        public Conversation Conversation { get; set; } = null!;

    }
}
