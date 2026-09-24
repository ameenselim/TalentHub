using MediatR;
using Microsoft.AspNetCore.Identity;
using TalentHub.Application.Common.Enums;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Services;

namespace TalentHub.Application.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ApiResponse<AuthResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ResetPasswordCommandHandler(UserManager<ApplicationUser> userManager, IAccountService accountService)
        {
            _userManager = userManager;
        }

        public async Task<ApiResponse<AuthResponse>> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(command.Request.UserNameOrEmail) ??
                 await _userManager.FindByNameAsync(command.Request.UserNameOrEmail);

            if (user is null)
            {
                return new ApiResponse<AuthResponse>()
                {
                    Success = false,
                    Message = "User not found."
                };
            }
            var result = await _userManager.ResetPasswordAsync(user, command.Request.ResetToken, command.Request.NewPassword);
            if (!result.Succeeded)
            {
                return new ApiResponse<AuthResponse>("Failed to reset password.", new List<string>(result.Errors.Select(x => x.Description)));
            }

            return new ApiResponse<AuthResponse>
            {
                Success = true,
                Message = "Password has been reset successfully."
            };
        }
    }   
}
