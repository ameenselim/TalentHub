using TalentHub.Domain.Common;
using TalentHub.Domain.Enums.Report;

namespace TalentHub.Domain.Entities
{
    public class Report :BaseEntity
    {
        public string ReporterId { get; set; } = null!;

        public int? JobId { get; set; }

        public int? CompanyId { get; set; }

        public string Reason { get; set; } = null!;

        public string? Description { get; set; }

        public ReportStatus Status { get; set; }

        public string? AdminNote { get; set; }

        // Navigation properties

        public Job? Job { get; set; }

        public Company? Company { get; set; }
    }
}
