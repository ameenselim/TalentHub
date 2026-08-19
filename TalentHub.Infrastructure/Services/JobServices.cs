using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using TalentHub.Application.Common.Extensions;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;

namespace TalentHub.Infrastructure.Services
{
    public class JobServices
    {
        private readonly IRepository<Job> _jobRepository;
        private readonly IRepository<Company> _companyRepository;
        public JobServices(IRepository<Job> jobRepository,IRepository<Company> companyRepository)
        {
            _jobRepository = jobRepository;
            _companyRepository = companyRepository;
        }
        public async Task<ApiResponse<JobResponse>> GetJobByIdAsync(int jobId)
        {
            var job = await _jobRepository.GetOneAsync(e => e.Id == jobId &&!e.IsDeleted, tracked: false,
                include: q => q.Include(e => e.Company)
                        .Include(e => e.Category)
                        .Include(e => e.JobSkills)
                            .ThenInclude(js => js.Skill)
                        .Include(e => e.JobRequirements));

            if(job is null)
            {
                return new ApiResponse<JobResponse>()
                {
                    Success = false,
                    Message = "Job not found",
                };
            }
            var response = new JobResponse()
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
            return new ApiResponse<JobResponse>(response,"Job retrieved successfully");
        }
        public async Task<PagedResponse<IEnumerable<JobListResponse>>> GetAllJobsAsync(PaginationRequest paginationRequest,
            JobFilterRequest jobFilter,
            CancellationToken cancellationToken = default)
        {
            paginationRequest.PageNumber = Math.Max(paginationRequest.PageNumber, 1);
            Expression<Func<Job, bool>> filter = e => !e.IsDeleted;
            if (jobFilter.MinSalary.HasValue)
            {
                filter = filter.And(e =>e.SalaryFrom.HasValue &&e.SalaryFrom >= jobFilter.MinSalary.Value);
            }
            if(jobFilter.MaxSalary.HasValue)
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
            if(jobFilter.WorkMode.HasValue)
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
            int totalPages = (int)Math.Ceiling((double)totalCount / paginationRequest.PageSize);
            var jobs = _jobRepository.GetAsync(
                expression: filter,
                tracked: false,
                page: paginationRequest.PageNumber,
                pageSize: paginationRequest.PageSize,
                cancellationToken: cancellationToken,
                include: q => q
                    .Include(e => e.Company)
                    .Include(e => e.Category)
                );
            return new PagedResponse<IEnumerable<JobListResponse>>()
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageSize = paginationRequest.PageSize,
                PageNumber = paginationRequest.PageNumber,
                Success = true,
                Message = "Jobs retrieved successfully",
                Data = (IEnumerable<IEnumerable<JobListResponse>>)jobs.Adapt<IEnumerable<JobListResponse>>()
            };
        }
    }
}
