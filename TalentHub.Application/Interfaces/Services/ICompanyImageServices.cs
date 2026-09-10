using System;
using System.Collections.Generic;
using System.Text;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;

namespace TalentHub.Application.Interfaces.Services
{
    public interface ICompanyImageServices
    {
        Task<ApiResponse<IEnumerable<CompanyImagesResponse>>> GetCompanyImagesAsync(int companyId, CancellationToken cancellationToken = default);
        Task<ApiResponse<CompanyImagesResponse>> GetCompanyImageByIdAsync(int imageId, int companyId, CancellationToken cancellationToken = default);
        Task<ApiResponse<CompanyImagesResponse>> AddImageAsync(string userId, int companyId, AddCompanyImageRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<CompanyImagesResponse>> UpdateImageAsync(string userId, int imageId, int companyId, UpdateCompanyImageRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> DeleteImageAsync(string userId, int imageId, int companyId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> SetAsCoverAsync(string userId, int companyId, int imageId, CancellationToken cancellationToken = default);
    }
}
