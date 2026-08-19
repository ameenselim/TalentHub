using System;
using System.Collections.Generic;
using System.Text;
using TalentHub.Domain.Enums.Experience;
using TalentHub.Domain.Enums.Job;

namespace TalentHub.Application.DTOs.Request
{
    public class JobFilterRequest
    {
        public string? Keyword { get; set; }

        public int? CategoryId { get; set; }

        public string? Country { get; set; }
        public string? City { get; set; }

        public WorkMode? WorkMode { get; set; }

        public ExperienceLevel? ExperienceLevel { get; set; }

        public decimal? MinSalary { get; set; }

        public decimal? MaxSalary { get; set; }
    }
}
}
