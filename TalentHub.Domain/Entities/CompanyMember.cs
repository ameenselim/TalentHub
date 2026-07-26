using TalentHub.Domain.Common;
using TalentHub.Domain.Enums.Company;

namespace TalentHub.Domain.Entities
{
    public class CompanyMember : BaseEntity
    {
        public string UserId { get; set; } = null!;

        public string? InvitedByUserId { get; set; }

        public int CompanyId { get; set; }

        public CompanyRole Role { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;


        //Navigation properties
        //public ApplicationUser User { get; set; } = null!;

        public Company Company { get; set; } = null!;

        //public ApplicationUser? Inviter { get; set; }

    }
}
