using TalentHub.Domain.Common;

namespace TalentHub.Domain.Entities
{
    public class Company : BaseEntity
    {
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public string? Logo { get; set; }
        public string? LogoPublicId { get; set; }

        public string? CoverImage { get; set; }
        public string? CoverImagePublicId { get; set; }

        public string? Website { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Country { get; set; }

        public string? City { get; set; }

        public string? Address { get; set; }

        public DateOnly? FoundedDate { get; set; }

        public string? Size { get; set; }

        public string? Industry { get; set; }

        public bool IsVerified { get; set; }

        // Navigation properties
        public ICollection<CompanyMember> Members { get; set; } = new HashSet<CompanyMember>();

        public ICollection<Job> Jobs { get; set; } = new HashSet<Job>();

        public ICollection<Review> Reviews { get; set; } = new HashSet<Review>();

        public ICollection<Report> Reports { get; set; } = new HashSet<Report>();

        public ICollection<CompanyFollower> Followers { get; set; } = new HashSet<CompanyFollower>();
        
        public ICollection<CompanyImage> Images { get; set; } = new HashSet<CompanyImage>();
    }
}
