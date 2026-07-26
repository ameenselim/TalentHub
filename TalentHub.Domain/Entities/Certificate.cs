using TalentHub.Domain.Common;

namespace TalentHub.Domain.Entities
{
    public class Certificate :BaseEntity
    {
        public int ResumeId { get; set; }

        public string Title { get; set; } = null!;

        public string Organization { get; set; } = null!;

        public DateOnly IssueDate { get; set; }

        public DateOnly? ExpirationDate { get; set; }

        public string? CertificateUrl { get; set; }

        // Navigation properties
        public Resume Resume { get; set; } = null!;
    }
}
