using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TalentHub.Application.Common.Enums;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Services;
using TalentHub.Domain.Enums.Company;

namespace TalentHub.Application.Features.Auth.Commands.RegisterCompany
{
    public class RegisterCompanyCommandHandler : IRequestHandler<RegisterCompanyCommand, ApiResponse<AuthResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<CompanyMember> _companyMemberRepository;
        private readonly IAccountService _accountService;
        private readonly ILogger<RegisterCompanyCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterCompanyCommandHandler(UserManager<ApplicationUser> userManager, IRepository<Company> companyRepository, IRepository<CompanyMember> companyMemberRepository, IAccountService accountService ,ILogger<RegisterCompanyCommandHandler> logger, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _companyRepository = companyRepository;
            _companyMemberRepository = companyMemberRepository;
            _accountService = accountService;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<AuthResponse>> Handle(RegisterCompanyCommand command, CancellationToken cancellationToken)
        {
            var emailExist = await _userManager.FindByEmailAsync(command.Request.Email);
            if (emailExist is not null)
            {
                return new ApiResponse<AuthResponse>("Registration failed.", new()
                {
                    "Business email already exists."
                });
            }
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                ApplicationUser user = new()
                {
                    FirstName = command.Request.FirstName,
                    LastName = command.Request.LastName,
                    UserName = command.Request.Email,
                    Email = command.Request.Email,
                    PhoneNumber = command.Request.PhoneNumber
                };
                var result = await _userManager.CreateAsync(user, command.Request.Password);
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return new ApiResponse<AuthResponse>("Registration failed.", result.Errors.Select(e => e.Description).ToList());
                }
                await _userManager.AddToRoleAsync(user, SystemRoles.CUSTOMER);
                Company company = new()
                {
                    Name = command.Request.CompanyName,
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
    }
}
