using MediatR;
using Microsoft.AspNetCore.Identity;
using TalentHub.Application.Common.Enums;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Services;

namespace TalentHub.Application.Features.Auth.Commands.ResendEmailConfirmation
{
    public class ResendEmailConfirmationCommandHandler : IRequestHandler<ResendEmailConfirmationCommand, ApiResponse<AuthResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAccountService _accountService;
        public ResendEmailConfirmationCommandHandler(UserManager<ApplicationUser> userManager, IAccountService accountService)
        {
            _userManager = userManager;
            _accountService = accountService;
        }

        public async Task<ApiResponse<AuthResponse>> Handle(ResendEmailConfirmationCommand command, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(command.Request.EmailOrUserName) ??
                await _userManager.FindByNameAsync(command.Request.EmailOrUserName);
            if (user == null)
            {
                return new ApiResponse<AuthResponse>()
                {
                    Message = "User not found.",
                    Success = false
                };
            }
            if (user.EmailConfirmed)
            {
                return new ApiResponse<AuthResponse>()
                {
                    Message = "Email is already confirmed.",
                    Success = false
                };
            }
            await _accountService.SendMailAsync(user.Id, EmailType.ResendConfirmation);
            return new ApiResponse<AuthResponse>()
            {
                Message = "Resend successfully.",
                Success = true
            };
        }
    }
}
