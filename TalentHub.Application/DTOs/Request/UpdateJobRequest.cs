using System.ComponentModel.DataAnnotations;
using TalentHub.Domain.Enums.Job;

namespace TalentHub.Application.DTOs.Request
{
    public class UpdateJobRequest
    {
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(150,MinimumLength = 2)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(5000,MinimumLength = 10)]
        public string Description { get; set; } = string.Empty;

        [StringLength(5000)]
        public string Responsibilities { get; set; } = string.Empty;

        [StringLength(1000)]        
        public string Country { get; set; } = string.Empty;

        [StringLength(100)]
        public string City { get; set; } =string.Empty;

        [Range(0,double.MaxValue)]
        public decimal? SalaryFrom { get; set; }

        [Range(0,double.MaxValue)]
        public decimal? SalaryTo { get; set; }

        [StringLength(10)]
        public string? Currency { get; set; }

        public bool IsSalaryVisible { get; set; } = true;

        [Required]
        public DateTime Deadline { get; set; }

        [Range(1,int.MaxValue)]
        public int Vacancies { get; set; } = 1;

        [EnumDataType(typeof(ExperienceLevel))]
        public ExperienceLevel ExperienceLevel { get; set; }

        [EnumDataType(typeof(WorkMode))]
        public WorkMode WorkMode { get; set; }

        [EnumDataType(typeof(JobType))]
        public JobType JobType { get; set; }

        [EnumDataType(typeof(JobStatus))]
        public JobStatus Status { get; set; }

        public List<string> Requirements { get; set; } = new();

        public List<int> SkillIds { get; set; } = new();
    }
}