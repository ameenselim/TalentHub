using TalentHub.Domain.Common;

namespace TalentHub.Domain.Entities
{
    public class RefreshToken :BaseEntity
    {
        public string UserId { get; set; } = null!;

        public string TokenHash { get; set; } = null!;

        public DateTime ExpiresOn { get; set; }

        public DateTime? RevokedOn { get; set; }

        public bool IsRevoked => RevokedOn != null;

        // Navigation Properties


    }
}
