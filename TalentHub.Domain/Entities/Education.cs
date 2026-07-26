using TalentHub.Domain.Common;

namespace TalentHub.Domain.Entities
{
    public class Education :BaseEntity
    {
        public int ResumeId { get; set; }

        public string University { get; set; } = null!;

        public string Faculty { get; set; } = null!;

        public string Degree { get; set; } = null!;

        public string? Grade { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        // Navigation properties
        public Resume Resume { get; set; } = null!;
    }
}
