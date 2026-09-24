using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Services;

namespace TalentHub.Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponse<AuthResponse>>
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Domain.Entities.RefreshToken> _refreshTokenRepository;
        private readonly IJwtService _jwtService;

        public RefreshTokenCommandHandler(IConfiguration configuration, UserManager<ApplicationUser> userManager, IRepository<Domain.Entities.RefreshToken> refreshTokenRepository, IJwtService jwtService)
        {
            _configuration = configuration;
            _userManager = userManager;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtService = jwtService;
        }

        public async Task<ApiResponse<AuthResponse>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var tokenHash = _jwtService.HashRefreshToken(command.Request.RefreshToken);
            var refreshToken = await _refreshTokenRepository.GetOneAsync(e => e.TokenHash == tokenHash);
            if (refreshToken is null)
            {
                return new ApiResponse<AuthResponse>()
                {
                    Success = false,
                    Message = "Invalid refresh token."
                };
            }
            if (refreshToken.IsRevoked)
            {
                return new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "Refresh token has been revoked."
                };
            }
            if (refreshToken.ExpiresOn <= DateTime.UtcNow)
            {
                return new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "Refresh token has expired."
                };
            }
            var user = await _userManager.FindByIdAsync(refreshToken.UserId);
            if (user is null)
            {
                return new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "Invalid refresh token."
                };
            }
            // Revoke old refresh token
            refreshToken.RevokedOn = DateTime.UtcNow;

            // Generate new access token
            var (accessToken, accessTokenExpiresAt) = await _jwtService.GenerateAccessTokenAsync(user);

            // Generate new refresh token
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            var newRefreshTokenEntity = new Domain.Entities.RefreshToken
            {
                UserId = user.Id,
                TokenHash = _jwtService.HashRefreshToken(newRefreshToken),
                CreatedAt = DateTime.UtcNow,
                ExpiresOn = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("JWT:RefreshTokenExpiryDays", 7)),
            };

            await _refreshTokenRepository.CreateAsync(newRefreshTokenEntity, cancellationToken);
            await _refreshTokenRepository.CommitAsync(cancellationToken);

            return new ApiResponse<AuthResponse>
            {
                Success = true,
                Message = "Token refreshed successfully.",
                Data = new AuthResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = newRefreshToken,
                    UserId = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    AccessTokenExpiresAt = accessTokenExpiresAt
                }
            };
        }
    }
}
