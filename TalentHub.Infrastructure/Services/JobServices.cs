using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq.Expressions;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Application.Interfaces.Services;
using TalentHub.Domain.Enums.Company;
using TalentHub.Domain.Enums.Job;
using TalentHub.Infrastructure.Extensions;

namespace TalentHub.Infrastructure.Services
{
    public class JobServices : IJobServices
    {
        private readonly IRepository<Job> _jobRepository;
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Skill> _skillRepository;
        private readonly IRepository<CompanyMember> _companyMemberRepository;
        public JobServices(IRepository<Job> jobRepository, IRepository<Company> companyRepository, IRepository<Category> categoryRepository, IRepository<Skill> skillRepository, IRepository<CompanyMember> companyMemberRepository)
        {
            _jobRepository = jobRepository;
            _companyRepository = companyRepository;
            _categoryRepository = categoryRepository;
            _skillRepository = skillRepository;
            _companyMemberRepository = companyMemberRepository;
            _skillRepository = skillRepository;
        }
        public async Task<ApiResponse<JobResponse>> GetJobByIdAsync(int jobId ,CancellationToken cancellationToken = default)
        {
            var job = await _jobRepository.GetOneAsync(e => e.Id == jobId && !e.IsDeleted, tracked: false,
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
        public async Task<ApiResponse<PagedResponse<JobListResponse>>> GetAllJobsAsync(JobFilterRequest jobFilter, CancellationToken cancellationToken = default)
        {
            jobFilter.PageNumber = Math.Max(jobFilter.PageNumber, 1);
            Expression<Func<Job, bool>> filter = e => !e.IsDeleted;
            if (jobFilter.MinSalary.HasValue)
            {
                filter = filter.And(e => e.SalaryFrom.HasValue && e.SalaryFrom >= jobFilter.MinSalary.Value);
            }
            if (jobFilter.MaxSalary.HasValue)
            {
                filter = filter.And(e => e.SalaryTo.HasValue && e.SalaryTo <= jobFilter.MaxSalary.Value);
            }
            if (!string.IsNullOrWhiteSpace(jobFilter.Keyword))
            {
                filter = filter.And(e => EF.Functions.Like(e.Title, $"%{jobFilter.Keyword.Trim()}%"));
            }
            if (jobFilter.ExperienceLevel.HasValue)
            {
                filter = filter.And(e => e.ExperienceLevel == jobFilter.ExperienceLevel.Value);
            }
            if (jobFilter.WorkMode.HasValue)
            {
                filter = filter.And(e => e.WorkMode == jobFilter.WorkMode.Value);
            }
            if (!string.IsNullOrWhiteSpace(jobFilter.Country))
            {
                filter = filter.And(e => e.Country == jobFilter.Country);
            }

            if (!string.IsNullOrWhiteSpace(jobFilter.City))
            {
                filter = filter.And(e => e.City == jobFilter.City);
            }

            int totalCount = await _jobRepository.CountAsync(filter, cancellationToken);
            int totalPages = (int)Math.Ceiling((double)totalCount / jobFilter.PageSize);
            var jobs = await _jobRepository.GetAsync(
                expression: filter,
                tracked: false,
                page: jobFilter.PageNumber,
                pageSize: jobFilter.PageSize,
                cancellationToken: cancellationToken,
                include: q => q
                    .Include(e => e.Company)
                    .Include(e => e.Category)
                );

            var pagedData = new PagedResponse<JobListResponse>
            {
                Data = jobs.Adapt<IEnumerable<JobListResponse>>(),
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageSize = jobFilter.PageSize,
                PageNumber = jobFilter.PageNumber
            };
            return new ApiResponse<PagedResponse<JobListResponse>>
            {
                Success = true,
                Message = "Jobs retrieved successfully",
                Data = pagedData
            };
        }
        public async Task<ApiResponse<JobResponse>> CreateJobAsync(string userId, CreateJobRequest request, CancellationToken cancellationToken = default)
        {
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

            return new ApiResponse<JobResponse>(MapToResponse(createdJob!), "Job created successfully");
        }

        public async Task<ApiResponse<JobResponse>> UpdateAsync(string userId, int jobId, UpdateJobRequest request, CancellationToken cancellationToken = default)
        {
            
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

            return new ApiResponse<JobResponse>(MapToResponse(updatedJob!), "Job updated successfully");
        }
        public async Task<ApiResponse<bool>> DeleteAsync(string userId, int jobId, CancellationToken cancellationToken = default)
        {
            var job = await _jobRepository.GetOneAsync(e => e.Id == jobId && !e.IsDeleted, cancellationToken: cancellationToken);
            if (job is null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Job not found"
                };
            }
            if (!await IsAuthorizedAsync(userId, job.CompanyId, cancellationToken))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "You are not authorized to delete this job."
                };
            }
            try
            {
                job.IsDeleted = true;
                job.DeletedAt = DateTime.UtcNow;
                _jobRepository.Update(job);
                await _jobRepository.CommitAsync(cancellationToken);
            }
            catch
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while deleting the job."
                };
            }

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Job deleted successfully",
            };
        }

        private JobResponse MapToResponse(Job job)
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
