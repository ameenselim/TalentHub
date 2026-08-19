using System;
using System.Collections.Generic;
using System.Text;
using TalentHub.Domain.Enums.Experience;
using TalentHub.Domain.Enums.Job;

namespace TalentHub.Application.DTOs.Response
{
    public class JobListResponse
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string Location { get; set; } = null!;

        public EmploymentType EmploymentType { get; set; }

        public WorkMode WorkMode { get; set; }

        public ExperienceLevel ExperienceLevel { get; set; }

        public decimal? SalaryMin { get; set; }

        public decimal? SalaryMax { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public CompanySummaryResponse Company { get; set; } = null!;

        public CategoryResponse Category { get; set; } = null!;
    }
}
