using MediatR;
using Microsoft.AspNetCore.Identity;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Services;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.Auth.Commands.ConfirmEmail
{
    public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, ApiResponse<AuthResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ConfirmEmailCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ApiResponse<AuthResponse>> Handle(ConfirmEmailCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.Token) || string.IsNullOrWhiteSpace(command.UserId))
            {
                return new ApiResponse<AuthResponse>("Token and UserId are required.");
            }
            var user = await _userManager.FindByIdAsync(command.UserId);
            if (user == null)
            {
                return new ApiResponse<AuthResponse>("User not found.");
            }
            var token = Uri.UnescapeDataString(command.Token);
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return new ApiResponse<AuthResponse>("Email confirmation failed.",
                    result.Errors.Select(e => e.Description).ToList());
            }
            return new ApiResponse<AuthResponse>(new AuthResponse
            {
                UserId = user.Id,
                UserName = user.UserName!,
                Email = user.Email!
            }, "Email confirmed successfully");
        }
    }
}
