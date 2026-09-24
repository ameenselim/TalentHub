using MediatR;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Services;

namespace TalentHub.Application.Features.Auth.Commands.Logout
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, ApiResponse<AuthResponse>>
    {
        private readonly IJwtService _jwtService;
        private readonly IRepository<Domain.Entities.RefreshToken> _refreshTokenRepository;
        public LogoutCommandHandler(IJwtService jwtService, IRepository<Domain.Entities.RefreshToken> refreshTokenRepository)
        {
            _jwtService = jwtService;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<ApiResponse<AuthResponse>> Handle(LogoutCommand command, CancellationToken cancellationToken)
        {
            var tokenHash = _jwtService.HashRefreshToken(command.Request.RefreshToken);
            var refreshToken = await _refreshTokenRepository.GetOneAsync(e => e.TokenHash == tokenHash);
            if (refreshToken is null)
            {
                return new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "Not Match"
                };
            }
            refreshToken.RevokedOn = DateTime.UtcNow;
            await _refreshTokenRepository.CommitAsync();

            return new ApiResponse<AuthResponse>
            {
                Success = true,
                Message = "Logged out successfully."
            };
        }
    }
}
