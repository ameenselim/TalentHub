namespace TalentHub.Domain.Entities
{
    public class SavedJob
    {
        public string UserId { get; set; } = null!;

        public int JobId { get; set; }

        public DateTime SavedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties

        public Job Job { get; set; } = null!;
    }
}
