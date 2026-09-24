using Microsoft.AspNetCore.Identity;
using TalentHub.Domain.Entities;

namespace TalentHub.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string? ProfileImage { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        public string? Country { get; set; }

        public string? City { get; set; }

        public string? Address { get; set; }

        public string? Headline { get; set; }

        public string? Bio { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? LastLoginAt { get; set; }

        //Navigation properties
        public ICollection<Resume> Resumes { get; set; } = new HashSet<Resume>();

        public ICollection<JobApplication> Applications { get; set; } = new HashSet<JobApplication>();

        public ICollection<UserSkill> UserSkills { get; set; } = new HashSet<UserSkill>();

        public ICollection<SavedJob> SavedJobs { get; set; } = new HashSet<SavedJob>();

        public ICollection<Notification> Notifications { get; set; } = new HashSet<Notification>();

        public ICollection<Review> Reviews { get; set; } = new HashSet<Review>();

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new HashSet<RefreshToken>();

        public ICollection<Message> Messages { get; set; } = new HashSet<Message>();

        public ICollection<CompanyMember> CompanyMemberships { get; set; } = new HashSet<CompanyMember>();

        public ICollection<Report> Reports { get; set; } = new HashSet<Report>();

        public ICollection<CompanyFollower> CompanyFollowers { get; set; } = new HashSet<CompanyFollower>();

        public ICollection<ApplicationUserOTP> ApplicationUserOTPs { get; set; } = new HashSet<ApplicationUserOTP>();
    }
}
