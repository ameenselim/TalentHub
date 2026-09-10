using System;
using System.Collections.Generic;
using System.Text;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;

namespace TalentHub.Application.Interfaces.Services
{
    public interface ISkillServices
    {
        Task<ApiResponse<IEnumerable<SkillResponse>>> GetAllSkillsAsync();
        Task<ApiResponse<SkillResponse>> GetSkillByIdAsync(int skillId);
        Task<ApiResponse<SkillResponse>> CreateAsync(string userId, CreateSkillRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<SkillResponse>> UpdateAsync(string userId, int id, UpdateSkillRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> DeleteAsync (int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> PermanentDeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
