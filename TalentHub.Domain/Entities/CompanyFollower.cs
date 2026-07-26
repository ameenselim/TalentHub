namespace TalentHub.Domain.Entities
{
    public class CompanyFollower
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public int CompanyId { get; set; }

        public DateTime FollowAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties

        public Company Company { get; set; } = null!;
    }
}
