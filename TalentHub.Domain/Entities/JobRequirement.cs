namespace TalentHub.Domain.Entities
{
    public class JobRequirement
    {
        public int Id { get; set; }
        public int JobId { get; set; }

        public string Requirement { get; set; } = null!;

        public int DisplayOrder { get; set; }

        // Navigation Properties
        public Job Job { get; set; } = null!;

    }
}
