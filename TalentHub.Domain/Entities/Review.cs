using TalentHub.Domain.Common;

namespace TalentHub.Domain.Entities
{
    public class Review :BaseEntity
    {
        public string UserId { get; set; } = null!;

        public int CompanyId { get; set; }

        public int Rate { get; set; }

        public string? Comment { get; set; }

        // Navigation Properties

        public Company Company { get; set; } = null!;
    }
}
