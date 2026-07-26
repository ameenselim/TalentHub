using TalentHub.Domain.Common;
using TalentHub.Domain.Enums.Interview;

namespace TalentHub.Domain.Entities
{
    public class Interview :BaseEntity
    {
        public int ApplicationId { get; set; }

        public DateTime InterviewDate { get; set; }

        public InterviewType Type { get; set; }

        public string? MeetingLink { get; set; }

        public string? Location { get; set; }

        public InterviewStatus Status { get; set; }

        public string? Notes { get; set; }

        // Navigation Properties
        public JobApplication Application { get; set; } = null!;
    }
}
