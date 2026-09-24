using MediatR;
using Microsoft.AspNetCore.Identity;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Services;

namespace TalentHub.Application.Features.Auth.Commands.ValidOTP
{
    public class ValidOTPCommandHandler : IRequestHandler<ValidOTPCommand, ApiResponse<AuthResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<ApplicationUserOTP> _applicationUserOTPRrepository;
        public ValidOTPCommandHandler(UserManager<ApplicationUser> userManager, IRepository<ApplicationUserOTP> applicationUserOTPRepository)
        {
            _userManager = userManager;
            _applicationUserOTPRrepository = applicationUserOTPRepository;
        }

        public async Task<ApiResponse<AuthResponse>> Handle(ValidOTPCommand command, CancellationToken cancellationToken)
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
            var otp = await _applicationUserOTPRrepository.GetOneAsync(e => e.ApplicationUserId == user.Id &&
                                        e.OTP == command.Request.OTP && !e.IsUsed);
            if (otp == null)
            {
                return new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "Invalid OTP."
                };
            }
            if (otp.ExpireAt <= DateTime.UtcNow)
            {
                return new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "OTP has expired."
                };
            }
            // OTP is valid.
            // Generate ASP.NET Identity password reset token.
            var resetToken =
                await _userManager.GeneratePasswordResetTokenAsync(user);

            // OTP can no longer be used.
            otp.IsUsed = true;
            otp.ExpireAt = DateTime.UtcNow;

            await _applicationUserOTPRrepository.CommitAsync();

            return new ApiResponse<AuthResponse>
            {
                Success = true,
                Message = "OTP is valid.",
                Data = new AuthResponse
                {
                    RefreshToken = resetToken,
                }
            };
        }
    }
}
