using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace TalentHub.Application.DTOs.Request
{
    public class CreateResumeRequest
    {
        [Required]
        [StringLength(1000, MinimumLength = 20)]
        public string? Summary { get; set; }

        [Range(0, 100000000)]
        public decimal? ExpectedSalary { get; set; }

        [Url]
        public string? PortfolioUrl { get; set; }

        [Required]
        [Url]
        public string? GithubUrl { get; set; }

        [Url]
        public string? LinkedinUrl { get; set; }

        [Range(0, 40)]
        public int YearsOfExperience { get; set; }
    }
}
