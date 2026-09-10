using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Domain.Entities;
using TalentHub.Domain.Enums.Job;

namespace TalentHub.Application.Interfaces.Services
{
    public interface IJobServices
    {

        Task<ApiResponse<JobResponse>> GetJobByIdAsync(int jobId, CancellationToken cancellationToken = default);

        Task<ApiResponse<PagedResponse<JobListResponse>>> GetAllJobsAsync(JobFilterRequest jobFilter, CancellationToken cancellationToken = default);

        Task<ApiResponse<JobResponse>> CreateJobAsync(string userId, CreateJobRequest request, CancellationToken cancellationToken = default);

        Task<ApiResponse<JobResponse>> UpdateAsync(string userId, int jobId, UpdateJobRequest request, CancellationToken cancellationToken = default);

        Task<ApiResponse<bool>> DeleteAsync(string userId, int jobId, CancellationToken cancellationToken = default);

    }
}
