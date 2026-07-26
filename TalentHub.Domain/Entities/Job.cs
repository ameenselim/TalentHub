using TalentHub.Domain.Common;
using TalentHub.Domain.Enums.Job;

namespace TalentHub.Domain.Entities
{
    public class Job : BaseEntity
    {
        public int CompanyId { get; set; }

        public int CategoryId { get; set; }

        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string Responsibilities { get; set; } = null!;

        public decimal? SalaryFrom { get; set; }

        public decimal? SalaryTo { get; set; }
        
        public string? Currency { get; set; }
        
        public bool IsSalaryVisible { get; set; } = true;

        public JobType JobType { get; set; }

        public ExperienceLevel ExperienceLevel { get; set; }

        public WorkMode WorkMode { get; set; }

        public string Country { get; set; } = null!;

        public string City { get; set; } = null!;

        public DateTime Deadline { get; set; }

        public int Vacancies { get; set; }

        public JobStatus Status { get; set; }

        public int ViewsCount { get; set; }

        public int ApplicationsCount { get; set; }

        // Navigation properties
        public Company Company { get; set; } = null!;

        public Category Category { get; set; } = null!;

        public ICollection<JobSkill> JobSkills { get; set; } = new HashSet<JobSkill>();
        
        public ICollection<JobRequirement> JobRequirements { get; set; } = new HashSet<JobRequirement>();

        public ICollection<JobApplication> Applications { get; set; } = new HashSet<JobApplication>();

        public ICollection<SavedJob> SavedJobs { get; set; } = new HashSet<SavedJob>();

        public ICollection<Report> Reports { get; set; } = new HashSet<Report>();

    }
}
