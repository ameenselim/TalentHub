using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Features.Jobs.Queries.GetJobById;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Domain.Entities;
using TalentHub.Domain.Enums.Company;
using TalentHub.Domain.Enums.Job;

namespace TalentHub.Application.Features.Jobs.Commands.CreateJob
{
    public record CreateJobCommand(string UserId, CreateJobRequest Request) : IRequest<ApiResponse<JobResponse>>;

    public class CreateJobCommandHandler : IRequestHandler<CreateJobCommand, ApiResponse<JobResponse>>
    {
        private readonly IRepository<Job> _jobRepository;
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Skill> _skillRepository;
        private readonly IRepository<CompanyMember> _companyMemberRepository;

        public CreateJobCommandHandler(
            IRepository<Job> jobRepository,
            IRepository<Company> companyRepository,
            IRepository<Category> categoryRepository,
            IRepository<Skill> skillRepository,
            IRepository<CompanyMember> companyMemberRepository)
        {
            _jobRepository = jobRepository;
            _companyRepository = companyRepository;
            _categoryRepository = categoryRepository;
            _skillRepository = skillRepository;
            _companyMemberRepository = companyMemberRepository;
        }

        public async Task<ApiResponse<JobResponse>> Handle(CreateJobCommand command, CancellationToken cancellationToken)
        {
            var userId = command.UserId;
            var request = command.Request;

            if (!await IsAuthorizedAsync(userId, request.CompanyId, cancellationToken))
            {
                return new ApiResponse<JobResponse>
                {
                    Success = false,
                    Message = "You are not authorized to create jobs for this company."
                };
            }
            var company = await _companyRepository.GetOneAsync(e => e.Id == request.CompanyId && !e.IsDeleted, tracked: false, cancellationToken: cancellationToken);
            if (company == null)
            {
                return new ApiResponse<JobResponse>
                {
                    Success = false,
                    Message = "Company not found",
                };
            }
            var category = await _categoryRepository.GetOneAsync(e => e.Id == request.CategoryId && !e.IsDeleted, tracked: false, cancellationToken: cancellationToken);
            if (category == null)
            {
                return new ApiResponse<JobResponse>
                {
                    Success = false,
                    Message = "Category not found",
                };
            }
            if (request.SalaryFrom.HasValue && request.SalaryTo.HasValue &&
                request.SalaryFrom.Value > request.SalaryTo.Value)
            {
                return new ApiResponse<JobResponse>
                {
                    Success = false,
                    Message = "SalaryFrom cannot be greater than SalaryTo"
                };
            }
            if (request.Deadline <= DateTime.UtcNow)
            {
                return new ApiResponse<JobResponse>
                {
                    Success = false,
                    Message = "Deadline must be in the future."
                };
            }
            var skillIds = request.SkillIds.Distinct().ToList();

            var skills = await _skillRepository.GetAsync(s => skillIds.Contains(s.Id) && !s.IsDeleted,
                tracked: false,
                cancellationToken: cancellationToken);

            var existingSkillIds = skills.Select(s => s.Id).ToHashSet();

            var invalidSkillIds = skillIds
                .Where(id => !existingSkillIds.Contains(id))
                .ToList();

            if (invalidSkillIds.Any())
            {
                return new ApiResponse<JobResponse>
                {
                    Success = false,
                    Message = $"The following SkillIds do not exist: {string.Join(", ", invalidSkillIds)}"
                };
            }
            var job = new Job
            {
                CompanyId = request.CompanyId,
                CategoryId = request.CategoryId,
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                Responsibilities = request.Responsibilities.Trim(),
                Country = request.Country.Trim(),
                City = request.City.Trim(),
                SalaryFrom = request.SalaryFrom,
                SalaryTo = request.SalaryTo,
                Currency = request.Currency?.Trim(),
                IsSalaryVisible = request.IsSalaryVisible,
                Deadline = request.Deadline,
                Vacancies = request.Vacancies,
                ExperienceLevel = request.ExperienceLevel,
                WorkMode = request.WorkMode,
                JobType = request.JobType,
                Status = JobStatus.Open,
                ViewsCount = 0,
                ApplicationsCount = 0,
                CreatedBy = userId,
                JobRequirements = request.Requirements.Where(r => !string.IsNullOrWhiteSpace(r))
                        .Select(r => r.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Select(r => new JobRequirement
                        {
                            Requirement = r
                        }).ToList(),
                JobSkills = skillIds.Select(skillId => new JobSkill { SkillId = skillId }).ToList()
            };
            try
            {
                await _jobRepository.CreateAsync(job, cancellationToken);
                await _jobRepository.CommitAsync(cancellationToken);
            }
            catch
            {
                return new ApiResponse<JobResponse>
                {
                    Success = false,
                    Message = "An error occurred while creating the job."
                };
            }

            var createdJob = await _jobRepository.GetOneAsync(e => e.Id == job.Id,
                tracked: false,
                cancellationToken: cancellationToken,
                include: q => q.Include(e => e.Company)
                    .Include(e => e.Category)
                    .Include(e => e.JobSkills).ThenInclude(js => js.Skill)
                    .Include(e => e.JobRequirements));

            return new ApiResponse<JobResponse>(GetJobByIdQueryHandler.MapToResponse(createdJob!), "Job created successfully");
        }

        private async Task<bool> IsAuthorizedAsync(string userId, int companyId, CancellationToken cancellationToken)
        {
            var member = await _companyMemberRepository.GetOneAsync(m => m.UserId == userId &&
                     m.CompanyId == companyId &&
                     m.IsActive &&
                     (m.Role == CompanyRole.Owner || m.Role == CompanyRole.Admin || m.Role == CompanyRole.Recruiter), tracked: false,
                     cancellationToken: cancellationToken);

            return member != null;
        }
    }
}
