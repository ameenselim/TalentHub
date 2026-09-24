using MediatR;
using Microsoft.Extensions.Logging;
using TalentHub.Application.Common.Enums;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.Auth.Commands.RegisterJobSeeker
{
    public class RegisterJobSeekerCommandHandler : IRequestHandler<RegisterJobSeekerCommand, ApiResponse<AuthResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAccountService _accountService;
        private readonly ILogger<RegisterJobSeekerCommandHandler> _logger;
        public RegisterJobSeekerCommandHandler(UserManager<ApplicationUser> userManager,IAccountService accountService,ILogger<RegisterJobSeekerCommandHandler> logger)
        {
            _userManager = userManager;
            _accountService = accountService;
            _logger = logger;
        }

        public async Task<ApiResponse<AuthResponse>> Handle(RegisterJobSeekerCommand command, CancellationToken cancellationToken)
        {
            var userExist = await _userManager.FindByEmailAsync(command.Request.Email);
            userExist ??= await _userManager.FindByNameAsync(command.Request.UserName);

            if (userExist is not null)
            {
                return new ApiResponse<AuthResponse>("Registration failed.", new()
                {
                    "Email or username already exists."
                });
            }
            ApplicationUser user = new()
            {
                FirstName = command.Request.FirstName,
                LastName = command.Request.LastName,
                UserName = command.Request.UserName,
                Email = command.Request.Email,
            };

            var result = await _userManager.CreateAsync(user, command.Request.Password);
            if (!result.Succeeded)
            {
                return new ApiResponse<AuthResponse>("Registration failed.",
                    result.Errors.Select(e => e.Description).ToList());
            }
            await _userManager.AddToRoleAsync(user, SystemRoles.CUSTOMER);
            try
            {
                await _accountService.SendMailAsync(user.Id, EmailType.Register);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email for user {UserId}", user.Id);
            }

            return new ApiResponse<AuthResponse>(new AuthResponse
            {
                UserId = user.Id,
                UserName = user.UserName!,
                Email = user.Email!
            }, "Registered successfully");
        }
    }
}
