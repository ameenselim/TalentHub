using Microsoft.VisualBasic;
using TalentHub.Domain.Common;
using TalentHub.Domain.Enums.Application;

namespace TalentHub.Domain.Entities
{
    public class JobApplication :BaseEntity
    {
        public string UserId { get; set; } = null!;

        public int JobId { get; set; }

        public int ResumeId { get; set; }

        public string? CoverLetter { get; set; }

        public ApplicationStatus Status { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties

        public Job Job { get; set; } = null!;

        public Resume Resume { get; set; } = null!;

        public Conversation? Conversation { get; set; }

        public ICollection<Interview> Interviews { get; set; } = new HashSet<Interview>();
    }
}
