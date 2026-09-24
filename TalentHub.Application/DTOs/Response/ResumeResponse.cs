using System;
using System.Collections.Generic;
using System.Text;

namespace TalentHub.Application.DTOs.Response
{
    public class ResumeResponse
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public decimal? ExpectedSalary { get; set; }
        public string? PortfolioUrl { get; set; }
        public string? GithubUrl { get; set; }
        public string? LinkedinUrl { get; set; }
        public string? ResumeFile { get; set; }
        public int YearsOfExperience { get; set; }

        public List<ExperienceResponse> Experiences { get; set; } = new();
        public List<EducationResponse> Educations { get; set; } = new();
        public List<CertificateResponse> Certificates { get; set; } = new();
    }
}
