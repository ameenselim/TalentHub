using TalentHub.Domain.Common;

namespace TalentHub.Domain.Entities
{
    public class Conversation :BaseEntity
    {
        public int ApplicationId { get; set; }

        // Navigation Properties
        public JobApplication Application { get; set; } = null!;

        public ICollection<Message> Messages { get; set; } = new HashSet<Message>();
    }
}
