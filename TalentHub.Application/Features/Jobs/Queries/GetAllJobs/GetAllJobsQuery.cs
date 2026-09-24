using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TalentHub.Application.Common.Extensions;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.Jobs.Queries.GetAllJobs
{
    public record GetAllJobsQuery(JobFilterRequest JobFilter) : IRequest<ApiResponse<PagedResponse<JobListResponse>>>;

    public class GetAllJobsQueryHandler : IRequestHandler<GetAllJobsQuery, ApiResponse<PagedResponse<JobListResponse>>>
    {
        private readonly IRepository<Job> _jobRepository;

        public GetAllJobsQueryHandler(IRepository<Job> jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task<ApiResponse<PagedResponse<JobListResponse>>> Handle(GetAllJobsQuery query, CancellationToken cancellationToken)
        {
            var jobFilter = query.JobFilter;
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
    }
}
