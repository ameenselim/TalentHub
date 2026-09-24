
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace TalentHub.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<AuthResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IJwtService _jwtService;
        private readonly IRepository<Domain.Entities.RefreshToken> _refreshTokenRepository;

        public LoginCommandHandler(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, IRepository<Domain.Entities.RefreshToken> refreshTokenRepository, IJwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtService = jwtService;
        }

        public async Task<ApiResponse<AuthResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(command.Request.UserNameOrEmail) ??
                await _userManager.FindByNameAsync(command.Request.UserNameOrEmail);

            if (user is null)
            {
                return new ApiResponse<AuthResponse>("Invalid username or password.", new List<string> { "Invalid username or password." });
            }
            var result = await _signInManager.CheckPasswordSignInAsync(user, command.Request.Password, true);
            if (result.IsNotAllowed)
            {
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    return new ApiResponse<AuthResponse>()
                    {
                        Message = "Please confirm your email before logging in.",
                        Success = false
                    };
                }

                return new ApiResponse<AuthResponse>()
                {
                    Message = "Login is not allowed for this account.",
                    Success = false
                };
            }
            if (result.IsLockedOut)
            {
                return new ApiResponse<AuthResponse>()
                {
                    Message = "Your account is locked. Please try again later.",
                    Success = false
                };
            }
            if (result.RequiresTwoFactor)
            {
                return new ApiResponse<AuthResponse>()
                {
                    Message = "Two-factor authentication is required.",
                    Success = false,
                    Data = new AuthResponse
                    {
                        UserId = user.Id,
                        UserName = user.UserName!,
                        Email = user.Email!
                    }
                };
            }
            if (!result.Succeeded)
            {
                return new ApiResponse<AuthResponse>("Invalid username or password.", new List<string> { "Invalid username or password." });
            }

            var (accessToken, accessTokenExpiresAt) = await _jwtService.GenerateAccessTokenAsync(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var refreshTokenEntity = new Domain.Entities.RefreshToken
            {
                UserId = user.Id,
                TokenHash = _jwtService.HashRefreshToken(refreshToken),
                CreatedAt = DateTime.UtcNow,
                ExpiresOn = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("JWT:RefreshTokenExpiryDays", 7)),
            };
            await _refreshTokenRepository.CreateAsync(refreshTokenEntity, cancellationToken);
            await _refreshTokenRepository.CommitAsync(cancellationToken);

            return new ApiResponse<AuthResponse>()
            {
                Success = true,
                Message = $"Welcome Back {user.FirstName} {user.LastName}",
                Data = new AuthResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    UserId = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    AccessTokenExpiresAt = accessTokenExpiresAt
                }
            };
        }
    }
}
