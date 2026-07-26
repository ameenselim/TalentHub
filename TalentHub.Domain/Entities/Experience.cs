using TalentHub.Domain.Common;
using TalentHub.Domain.Enums.Experience;

namespace TalentHub.Domain.Entities
{
    public class Experience :BaseEntity
    {
        public int ResumeId { get; set; }

        public string CompanyName { get; set; } = null!;

        public string Position { get; set; } = null!;

        public EmploymentType EmploymentType { get; set; }

        public string? Description { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public bool IsCurrent { get; set; }

        // Navigation properties
        public Resume Resume { get; set; } = null!;
    }
}
