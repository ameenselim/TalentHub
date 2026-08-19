using System;
using System.Collections.Generic;
using System.Text;
using TalentHub.Domain.Enums.Job;

namespace TalentHub.Application.DTOs.Response
{
    public class JobResponse
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = null!;

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;

        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Responsibilities { get; set; } = null!;

        public decimal? SalaryFrom { get; set; }
        public decimal? SalaryTo { get; set; }
        public string? Currency { get; set; }
        public bool IsSalaryVisible { get; set; }

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

        public List<string> Skills { get; set; } = [];
        public List<string> Requirements { get; set; } = [];
    }
}
