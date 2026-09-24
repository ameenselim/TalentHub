using System.Threading.Tasks;
using TalentHub.Application.DTOs.Response;

namespace TalentHub.Application.Interfaces.Services
{
    public interface IProfileServices
    {
        Task<ApiResponse<UserProfileResponse>> GetUserProfileAsync(string userId);
        Task<ApiResponse<CompanyProfileResponse>> GetCompanyProfileAsync(int companyId);
    }
}
