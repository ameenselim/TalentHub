using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.Resumes.Queries.GetAllResumesByUserId
{
    public record GetAllResumesByUserIdQuery(string UserId) : IRequest<ApiResponse<IEnumerable<ResumeResponse>>>;

    public class GetAllResumesByUserIdQueryHandler : IRequestHandler<GetAllResumesByUserIdQuery, ApiResponse<IEnumerable<ResumeResponse>>>
    {
        private readonly IRepository<Resume> _resumeRepository;

        public GetAllResumesByUserIdQueryHandler(IRepository<Resume> resumeRepository)
        {
            _resumeRepository = resumeRepository;
        }

        public async Task<ApiResponse<IEnumerable<ResumeResponse>>> Handle(GetAllResumesByUserIdQuery query, CancellationToken cancellationToken)
        {
            var resumes = await _resumeRepository.GetAsync(
                e => e.UserId == query.UserId && !e.IsDeleted,
                tracked: false,
                cancellationToken: cancellationToken,
                include: q => q.Include(e => e.Experiences)
                                .Include(e => e.Educations)
                                .Include(e => e.Certificates));

            return new ApiResponse<IEnumerable<ResumeResponse>>
            {
                Success = true,
                Data = resumes?.Select(MapToResponse) ?? Enumerable.Empty<ResumeResponse>()
            };
        }

        public static ResumeResponse MapToResponse(Resume resume)
        {
            return new ResumeResponse
            {
                Id = resume.Id,
                UserId = resume.UserId,
                Summary = resume.Summary,
                ExpectedSalary = resume.ExpectedSalary,
                PortfolioUrl = resume.PortfolioUrl,
                GithubUrl = resume.GithubUrl,
                LinkedinUrl = resume.LinkedinUrl,
                ResumeFile = resume.ResumeFile,
                YearsOfExperience = resume.YearsOfExperience,
                Experiences = resume.Experiences?.Select(e => new ExperienceResponse
                {
                    Id = e.Id,
                    CompanyName = e.CompanyName,
                    Position = e.Position,
                    EmploymentType = e.EmploymentType.ToString(),
                    Description = e.Description,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    IsCurrent = e.IsCurrent
                }).ToList() ?? new List<ExperienceResponse>(),
                Educations = resume.Educations?.Select(e => new EducationResponse
                {
                    Id = e.Id,
                    University = e.University,
                    Faculty = e.Faculty,
                    Degree = e.Degree,
                    Grade = e.Grade,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate
                }).ToList() ?? new List<EducationResponse>(),
                Certificates = resume.Certificates?.Select(c => new CertificateResponse
                {
                    Id = c.Id,
                    Title = c.Title,
                    Organization = c.Organization,
                    IssueDate = c.IssueDate,
                    ExpirationDate = c.ExpirationDate,
                    CertificateUrl = c.CertificateUrl
                }).ToList() ?? new List<CertificateResponse>()
            };
        }
    }
}
