using System;
using System.Collections.Generic;
using System.Text;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;

namespace TalentHub.Application.Interfaces.Services
{
    public interface IAuthServices
    {
        Task<ApiResponse<AuthResponse>> RegisterAsJobSeekerAsync(RegisterUserRequest request);

        Task<ApiResponse<AuthResponse>> RegisterAsCompanyAsync(RegisterCompanyRequest request, CancellationToken cancellationToken = default);

        Task<ApiResponse<AuthResponse>> ConfirmEmail(string userId, string token);

        Task<ApiResponse<AuthResponse>> ResendEmailConfirmation(ResendEmailConfirmationRequest request);
    }
}
