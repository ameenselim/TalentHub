using System.ComponentModel.DataAnnotations;
using TalentHub.Domain.Enums.Job;

namespace TalentHub.Application.DTOs.Request
{
    public class CreateJobRequest
    {
        public int CompanyId { get; set; }

        public int CategoryId { get; set; }

        [Required]
        [StringLength(150, MinimumLength = 2)]        
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(5000, MinimumLength = 10)]
        public string Description { get; set; } = string.Empty;

        [StringLength(5000)]   
        public string Responsibilities { get; set; } = string.Empty;

        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "SalaryFrom cannot be negative.")]
        public decimal? SalaryFrom { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "SalaryTo cannot be negative.")]
        public decimal? SalaryTo { get; set; }

        public string? Currency { get; set; }

        public bool IsSalaryVisible { get; set; } = true;

        [DataType(DataType.Date)]
        public DateTime Deadline { get; set; }

        public int Vacancies { get; set; } = 1;

        [EnumDataType(typeof(ExperienceLevel),ErrorMessage = "Invalid experience level.")]
        public ExperienceLevel ExperienceLevel { get; set; }

        [EnumDataType(typeof(WorkMode),ErrorMessage = "Invalid work mode.")]
        public WorkMode WorkMode { get; set; }

        [EnumDataType(typeof(JobType),ErrorMessage = "Invalid job type.")]
        public JobType JobType { get; set; }

        public List<string> Requirements { get; set; } = new();

        public List<int> SkillIds { get; set; } = new();
    }
}