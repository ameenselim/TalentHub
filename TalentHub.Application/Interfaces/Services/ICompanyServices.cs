using System;
using System.Collections.Generic;
using System.Text;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;

namespace TalentHub.Application.Interfaces.Services
{
    public interface ICompanyServices
    {
        Task<ApiResponse<PagedResponse<CompanyListResponse>>> GetAllCompaniesAsync(CompanyFilterRequest companyRequest, CancellationToken cancellationToken = default);
        Task<ApiResponse<CompanyResponse>> GetCompanyByIdAsync(int companyId, CancellationToken cancellationToken = default);
        Task<ApiResponse<CompanyResponse>> UpdateAsync(string userId, int id, UpdateCompanyRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> DeleteAsync(string userId, int id, CancellationToken cancellationToken = default);
    }
}
