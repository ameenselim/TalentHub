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

        Task<ApiResponse<AuthResponse>> LoginAsync(Application.DTOs.Request.LoginRequest request, CancellationToken cancellationToken = default);

        Task<ApiResponse<AuthResponse>> ForgetPasswordAsync(ForgetPasswordRequest request);

        Task<ApiResponse<AuthResponse>> ValidOTPAsync(ValidOTPRequest request);

        Task<ApiResponse<AuthResponse>> ResetPasswordAsync(ResetPasswordRequest request);

        Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
        
        Task<ApiResponse<AuthResponse>> LogoutAsync(RefreshTokenRequest request);
    }
}
