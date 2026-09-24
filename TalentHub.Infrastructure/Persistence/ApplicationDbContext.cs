using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TalentHub.Infrastructure.Persistence
{
    public class ApplicationDbContext :IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Company> Companies { get; set; } = null!;
        public DbSet<CompanyMember> CompanyMembers { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;    
        public DbSet<Skill> Skills { get; set; } = null!;
        public DbSet<Job> Jobs { get; set; } = null!;
        public DbSet<JobSkill> JobSkills { get; set; } = null!;
        public DbSet<UserSkill> UserSkills { get; set; } = null!;
        public DbSet<SavedJob> SavedJobs { get; set; } = null!;
        public DbSet<Resume> Resumes { get; set; } = null!;
        public DbSet<Experience> Experiences { get; set; } = null!;
        public DbSet<Education> Educations { get; set; } = null!;
        public DbSet<Certificate> Certificates { get; set; } = null!;
        public DbSet<JobApplication> Applications { get; set; } = null!;
        public DbSet<Interview> Interviews { get; set; } = null!;
        public DbSet<Conversation> Conversations { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<Report> Reports { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<JobRequirement> JobRequirements { get; set; } = null!;
        public DbSet<CompanyImage> CompanyImages { get; set; } = null!;
        public DbSet<CompanyFollower> CompanyFollowers { get; set; } = null!;
        public DbSet<ApplicationUserOTP> ApplicationUserOTPs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(JobTypeConfigurations).Assembly);
        }

    }
}