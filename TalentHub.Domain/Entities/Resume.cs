using TalentHub.Domain.Common;

namespace TalentHub.Domain.Entities
{
    public class Resume :BaseEntity
    {
        public string UserId { get; set; } = null!;

        public string? Summary { get; set; }

        public decimal? ExpectedSalary { get; set; }

        public string? PortfolioUrl { get; set; }

        public string? GithubUrl { get; set; }

        public string? LinkedinUrl { get; set; }

        public string? ResumeFile { get; set; }
        public string? ResumePublicId { get; set; }

        public int YearsOfExperience { get; set; }

        // Navigation Properties

        public ICollection<Experience> Experiences { get; set; } = new HashSet<Experience>();

        public ICollection<Education> Educations { get; set; } = new HashSet<Education>();

        public ICollection<Certificate> Certificates { get; set; } = new HashSet<Certificate>();

        public ICollection<JobApplication> Applications { get; set; } = new HashSet<JobApplication>();
    }
}
