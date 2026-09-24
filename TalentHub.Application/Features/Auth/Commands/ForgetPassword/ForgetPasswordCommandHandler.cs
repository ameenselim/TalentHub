using MediatR;
using Microsoft.AspNetCore.Identity;
using TalentHub.Application.Common.Enums;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Services;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.Auth.Commands.ForgetPassword
{
    public class ForgetPasswordCommandHandler : IRequestHandler<ForgetPasswordCommand, ApiResponse<AuthResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAccountService _accountService;

        public ForgetPasswordCommandHandler(UserManager<ApplicationUser> userManager, IAccountService accountService)
        {
            _userManager = userManager;
            _accountService = accountService;
        }

        public async Task<ApiResponse<AuthResponse>> Handle(ForgetPasswordCommand command, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(command.Request.UserNameOrEmail) ??
                await _userManager.FindByNameAsync(command.Request.UserNameOrEmail);

            if (user is not null)
                await _accountService.SendMailAsync(user.Id, EmailType.ForgetPassword);

            return new ApiResponse<AuthResponse>()
            {
                Success = true,
                Message = "If the user exists, a password reset email has been sent.",
                Data = new AuthResponse
                {
                    UserName = user?.UserName ?? string.Empty,
                    Email = user?.Email ?? string.Empty
                }
            };
        }
    }
}
