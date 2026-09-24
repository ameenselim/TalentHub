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

namespace TalentHub.Application.Features.Jobs.Commands.UpdateJob
{
    public record UpdateJobCommand(string UserId, int JobId, UpdateJobRequest Request) : IRequest<ApiResponse<JobResponse>>;

    public class UpdateJobCommandHandler : IRequestHandler<UpdateJobCommand, ApiResponse<JobResponse>>
    {
        private readonly IRepository<Job> _jobRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Skill> _skillRepository;
        private readonly IRepository<CompanyMember> _companyMemberRepository;

        public UpdateJobCommandHandler(
            IRepository<Job> jobRepository,
            IRepository<Category> categoryRepository,
            IRepository<Skill> skillRepository,
            IRepository<CompanyMember> companyMemberRepository)
        {
            _jobRepository = jobRepository;
            _categoryRepository = categoryRepository;
            _skillRepository = skillRepository;
            _companyMemberRepository = companyMemberRepository;
        }

        public async Task<ApiResponse<JobResponse>> Handle(UpdateJobCommand command, CancellationToken cancellationToken)
        {
            var userId = command.UserId;
            var jobId = command.JobId;
            var request = command.Request;

            var job = await _jobRepository.GetOneAsync(e => e.Id == jobId && !e.IsDeleted, cancellationToken: cancellationToken,
                include: q => q.Include(e => e.JobRequirements)
                                            .Include(e => e.JobSkills));
            if (job is null)
            {
                return new ApiResponse<JobResponse>
                {
                    Success = false,
                    Message = "Job not found"
                };
            }
            if (!await IsAuthorizedAsync(userId, job.CompanyId, cancellationToken))
            {
                return new ApiResponse<JobResponse>
                {
                    Success = false,
                    Message = "You are not authorized to update this job."
                };
            }

            var category = await _categoryRepository.GetOneAsync(e => e.Id == request.CategoryId && !e.IsDeleted,
                tracked: false, cancellationToken: cancellationToken);
            if (category is null)
            {
                return new ApiResponse<JobResponse>
                {
                    Success = false,
                    Message = "Category not found"
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

            var invalidSkillIds = skillIds.Where(id => !existingSkillIds.Contains(id)).ToList();
            if (invalidSkillIds.Any())
            {
                return new ApiResponse<JobResponse>
                {
                    Success = false,
                    Message = $"The following SkillIds do not exist: {string.Join(", ", invalidSkillIds)}"
                };
            }

            try
            {
                job.CategoryId = request.CategoryId;
                job.Title = request.Title.Trim();
                job.Description = request.Description.Trim();
                job.Responsibilities = request.Responsibilities.Trim();
                job.Country = request.Country.Trim();
                job.City = request.City.Trim();
                job.SalaryFrom = request.SalaryFrom;
                job.SalaryTo = request.SalaryTo;
                job.Currency = request.Currency?.Trim();
                job.IsSalaryVisible = request.IsSalaryVisible;
                job.Deadline = request.Deadline;
                job.Vacancies = request.Vacancies;
                job.ExperienceLevel = request.ExperienceLevel;
                job.WorkMode = request.WorkMode;
                job.JobType = request.JobType;
                job.Status = request.Status;
                job.UpdatedAt = DateTime.UtcNow;
                job.UpdatedBy = userId;

                // استبدال الـ Requirements بالكامل (Simplest approach)
                job.JobRequirements.Clear();
                var requirements = request.Requirements.Where(r => !string.IsNullOrWhiteSpace(r))
                    .Select(r => r.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase);

                foreach (var requirement in requirements)
                {
                    job.JobRequirements.Add(new JobRequirement
                    {
                        Requirement = requirement
                    });
                }

                // استبدال الـ Skills بالكامل
                job.JobSkills.Clear();
                foreach (var skillId in request.SkillIds.Distinct())
                {
                    job.JobSkills.Add(new JobSkill { SkillId = skillId });
                }

                _jobRepository.Update(job);
                await _jobRepository.CommitAsync(cancellationToken);
            }
            catch
            {
                return new ApiResponse<JobResponse>
                {
                    Success = false,
                    Message = "An error occurred while updating the job."
                };
            }

            var updatedJob = await _jobRepository.GetOneAsync(e => e.Id == jobId,
                tracked: false,
                cancellationToken: cancellationToken,
                include: q => q.Include(e => e.Company)
                    .Include(e => e.Category)
                    .Include(e => e.JobSkills).ThenInclude(js => js.Skill)
                    .Include(e => e.JobRequirements));

            return new ApiResponse<JobResponse>(GetJobByIdQueryHandler.MapToResponse(updatedJob!), "Job updated successfully");
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
