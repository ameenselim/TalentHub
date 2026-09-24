using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.Jobs.Queries.GetJobById
{
    public record GetJobByIdQuery(int JobId) : IRequest<ApiResponse<JobResponse>>;

    public class GetJobByIdQueryHandler : IRequestHandler<GetJobByIdQuery, ApiResponse<JobResponse>>
    {
        private readonly IRepository<Job> _jobRepository;

        public GetJobByIdQueryHandler(IRepository<Job> jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task<ApiResponse<JobResponse>> Handle(GetJobByIdQuery query, CancellationToken cancellationToken)
        {
            var job = await _jobRepository.GetOneAsync(e => e.Id == query.JobId && !e.IsDeleted, tracked: false,
                cancellationToken: cancellationToken,
                include: q => q.Include(e => e.Company)
                        .Include(e => e.Category)
                        .Include(e => e.JobSkills)
                            .ThenInclude(js => js.Skill)
                        .Include(e => e.JobRequirements));

            if (job is null)
            {
                return new ApiResponse<JobResponse>
                {
                    Success = false,
                    Message = "Job not found",
                };
            }
            return new ApiResponse<JobResponse>(MapToResponse(job), "Job retrieved successfully");
        }

        public static JobResponse MapToResponse(Job job)
        {
            return new JobResponse
            {
                Id = job.Id,
                CategoryId = job.CategoryId,
                CategoryName = job.Category.Name,
                SalaryFrom = job.SalaryFrom,
                SalaryTo = job.SalaryTo,
                City = job.City,
                Status = job.Status,
                Description = job.Description,
                Deadline = job.Deadline,
                IsSalaryVisible = job.IsSalaryVisible,
                ApplicationsCount = job.ApplicationsCount,
                Country = job.Country,
                ExperienceLevel = job.ExperienceLevel,
                Title = job.Title,
                Vacancies = job.Vacancies,
                ViewsCount = job.ViewsCount,
                Currency = job.Currency,
                WorkMode = job.WorkMode,
                JobType = job.JobType,
                CompanyId = job.CompanyId,
                CompanyName = job.Company.Name,
                Responsibilities = job.Responsibilities,
                Requirements = job.JobRequirements.Select(e => e.Requirement).ToList(),
                Skills = job.JobSkills.Select(e => e.Skill.Name).ToList()
            };
        }
    }
}
