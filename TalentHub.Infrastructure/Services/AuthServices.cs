using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;
using TalentHub.Application.Common.Enums;
using TalentHub.Application.Common.Settings;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Application.Interfaces.Services;
using TalentHub.Domain.Enums.Company;

namespace TalentHub.Infrastructure.Services
{
    public class AuthServices
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAccountService _accountService;
        private readonly ILogger<AuthServices> _logger;
        private readonly IRepository<CompanyMember> _companyMemberRepository;
        private readonly IRepository<Company> _companyRepository;
        private readonly ApplicationDbContext _context;

        public AuthServices(UserManager<ApplicationUser> userManager, IAccountService accountService, ILogger<AuthServices> logger, IRepository<CompanyMember> companyMemberRepository, IRepository<Company> companyRepository, ApplicationDbContext context )
        {
            _userManager = userManager;
            _accountService = accountService;
            _logger = logger;
            _companyMemberRepository = companyMemberRepository;
            _companyRepository = companyRepository;
            _context = context;
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

                Company company = new()
                {
                    Name = request.CompanyName,
                };
                await _companyRepository.CreateAsync(company, cancellationToken);

                CompanyMember companyMember = new()
                {
                    CompanyId = company.Id,
                    UserId = user.Id,
                    Role = CompanyRole.Owner
                };
                await _companyMemberRepository.CreateAsync(companyMember, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
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

    }

}
