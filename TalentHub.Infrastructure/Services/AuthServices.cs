using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TalentHub.Application.Common.Enums;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Application.Interfaces.Services;
using TalentHub.Domain.Enums.Company;
using TalentHub.Infrastructure.Utilities;

namespace TalentHub.Infrastructure.Services
{
    public class AuthServices : IAuthServices
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<RefreshToken> _refreshTokenRepository;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAccountService _accountService;
        private readonly ILogger<AuthServices> _logger;
        private readonly IRepository<CompanyMember> _companyMemberRepository;
        private readonly IRepository<Company> _companyRepository;
        private readonly ApplicationDbContext _context;
        private readonly IRepository<ApplicationUserOTP> _applicationUserOTPRrepository;



        public AuthServices(UserManager<ApplicationUser> userManager,IRepository<RefreshToken> refreshTokenRepository, IRepository<ApplicationUserOTP> applicationUserOTPRepository, IConfiguration configuration, SignInManager<ApplicationUser> signInManager, IAccountService accountService, ILogger<AuthServices> logger, IRepository<CompanyMember> companyMemberRepository, IRepository<Company> companyRepository, ApplicationDbContext context)
        {
            _userManager = userManager;
            _refreshTokenRepository = refreshTokenRepository;
            _configuration = configuration;
            _signInManager = signInManager;
            _accountService = accountService;
            _logger = logger;
            _companyMemberRepository = companyMemberRepository;
            _companyRepository = companyRepository;
            _context = context;
            _applicationUserOTPRrepository = applicationUserOTPRepository;
        }

        public async Task<ApiResponse<AuthResponse>> RegisterAsJobSeekerAsync(RegisterUserRequest request)
        {
            var userExist = await _userManager.FindByEmailAsync(request.Email);
            userExist ??= await _userManager.FindByNameAsync(request.UserName);

            if (userExist is not null)
            {
                return new ApiResponse<AuthResponse>("Registration failed.", new()
                {
                    "Email or username already exists."
                });
            }
            ApplicationUser user = new()
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.UserName,
                Email = request.Email,
            };

            var result = await _userManager.CreateAsync(user, request.Password);
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
        public async Task<ApiResponse<AuthResponse>> RegisterAsCompanyAsync(RegisterCompanyRequest request, CancellationToken cancellationToken = default)
        {
            var emailExist = await _userManager.FindByEmailAsync(request.Email);
            if (emailExist is not null)
            {
                return new ApiResponse<AuthResponse>("Registration failed.", new()
                {
                    "Business email already exists."
                });
            }
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                ApplicationUser user = new()
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    UserName = request.Email,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber
                };
                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return new ApiResponse<AuthResponse>("Registration failed.", result.Errors.Select(e => e.Description).ToList());
                }
                await _userManager.AddToRoleAsync(user, SystemRoles.CUSTOMER);
                Company company = new()
                {
                    Name = request.CompanyName,
                };
                await _companyRepository.CreateAsync(company, cancellationToken);
                await _companyRepository.CommitAsync(cancellationToken);

                CompanyMember companyMember = new()
                {
                    CompanyId = company.Id,
                    UserId = user.Id,
                    Role = CompanyRole.Owner
                };
                await _companyMemberRepository.CreateAsync(companyMember, cancellationToken);
                await _companyMemberRepository.CommitAsync(cancellationToken);

                await transaction.CommitAsync();

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
                }, "Company Account Created Successfully ,Please Verify Your Email");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        public async Task<ApiResponse<AuthResponse>> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(userId))
            {
                return new ApiResponse<AuthResponse>("Token and UserId are required.");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new ApiResponse<AuthResponse>("User not found.");
            }
            token = Uri.UnescapeDataString(token);
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
        public async Task<ApiResponse<AuthResponse>> ResendEmailConfirmation(ResendEmailConfirmationRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.EmailOrUserName) ??
                await _userManager.FindByNameAsync(request.EmailOrUserName);
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

        public async Task<ApiResponse<AuthResponse>> LoginAsync(Application.DTOs.Request.LoginRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(request.UserNameOrEmail) ??
                await _userManager.FindByNameAsync(request.UserNameOrEmail);

            if (user is null)
            {
                return new ApiResponse<AuthResponse>("Invalid username or password.", new List<string> { "Invalid username or password." });
            }
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);
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

            var (accessToken, accessTokenExpiresAt) = await GenerateAccessTokenAsync(user);
            var refreshToken = GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = HashRefreshToken(refreshToken),
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
        public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request , CancellationToken cancellationToken = default)
        {
            var tokenHash = HashRefreshToken(request.RefreshToken);
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
            var (accessToken, accessTokenExpiresAt) = await GenerateAccessTokenAsync(user);

            // Generate new refresh token
            var newRefreshToken = GenerateRefreshToken();

            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = HashRefreshToken(newRefreshToken),
                CreatedAt = DateTime.UtcNow,
                ExpiresOn = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("JWT:RefreshTokenExpiryDays",7)),                         
            };

            await _refreshTokenRepository.CreateAsync(newRefreshTokenEntity,cancellationToken);
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

        public async Task<ApiResponse<AuthResponse>> ForgetPasswordAsync(ForgetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.UserNameOrEmail) ??
                await _userManager.FindByNameAsync(request.UserNameOrEmail);

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
        public async Task<ApiResponse<AuthResponse>> ValidOTPAsync(ValidOTPRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.UserNameOrEmail) ??
               await _userManager.FindByNameAsync(request.UserNameOrEmail);

            if (user is null)
            {
                return new ApiResponse<AuthResponse>()
                {
                    Success = false,
                    Message = "User not found."
                };
            }
            var otp = await _applicationUserOTPRrepository.GetOneAsync(e => e.ApplicationUserId == user.Id &&
                                        e.OTP == request.OTP && !e.IsUsed);
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
        public async Task<ApiResponse<AuthResponse>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.UserNameOrEmail) ??
                await _userManager.FindByNameAsync(request.UserNameOrEmail);

            if (user is null)
            {
                return new ApiResponse<AuthResponse>()
                {
                    Success = false,
                    Message = "User not found."
                };
            }
            var result = await _userManager.ResetPasswordAsync(user, request.ResetToken, request.NewPassword);
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
        public async Task<ApiResponse<AuthResponse>> LogoutAsync(RefreshTokenRequest request)
        {
            var tokenHash = HashRefreshToken(request.RefreshToken);
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

        private string GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }
        private string HashRefreshToken(string refreshToken)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
            return Convert.ToBase64String(hash);
        }
        private async Task<(string Token, DateTime ExpiresAt)> GenerateAccessTokenAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id),
                new Claim(ClaimTypes.Name,$"{user.FirstName} {user.LastName}"),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };

            var roles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var expiryMinutes = _configuration.GetValue<int>("JWT:AccessTokenExpiryMinutes", 10);
            var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:issuer"],
                audience: _configuration["JWT:audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
                );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }

}
