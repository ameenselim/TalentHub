namespace TalentHub.Domain.Entities
{
    public class CompanyImage
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }

        public string ImageUrl { get; set; } = null!;
        public string ImagePublicId { get; set; } = null!;

        public string? Caption { get; set; }

        public bool IsCover { get; set; }

        // Navigation Properties
        public Company Company { get; set; } = null!;
    }
}
