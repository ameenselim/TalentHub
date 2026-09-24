using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;

namespace TalentHub.Application.Interfaces.Services
{
    public interface IResumeServices
    {
        Task<ApiResponse<IEnumerable<ResumeResponse>>> GetAllByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<ResumeResponse>> GetByIdAsync(int resumeId, CancellationToken cancellationToken = default);
        Task<ApiResponse<ResumeResponse>> CreateAsync(string userId, CreateResumeRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<ResumeResponse>> UpdateAsync(string userId, int resumeId, UpdateResumeRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> DeleteAsync(string userId, int resumeId, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> UploadResumeFileAsync(string userId, int resumeId, IFormFile file, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> DeleteResumeFileAsync(string userId, int resumeId, CancellationToken cancellationToken = default);
    }
}
