using MediatR;
using System;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Domain.Entities;
using TalentHub.Domain.Enums.Company;

namespace TalentHub.Application.Features.Companies.Commands.DeleteCompany
{
    public record DeleteCompanyCommand(string UserId, int CompanyId) : IRequest<ApiResponse<bool>>;

    public class DeleteCompanyCommandHandler : IRequestHandler<DeleteCompanyCommand, ApiResponse<bool>>
    {
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<CompanyMember> _companyMemberRepository;

        public DeleteCompanyCommandHandler(IRepository<Company> companyRepository, IRepository<CompanyMember> companyMemberRepository)
        {
            _companyRepository = companyRepository;
            _companyMemberRepository = companyMemberRepository;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteCompanyCommand command, CancellationToken cancellationToken)
        {
            var userId = command.UserId;
            var companyId = command.CompanyId;

            if (!await IsAuthorizedAsync(userId, companyId, cancellationToken))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "You are not authorized to delete this company."
                };
            }
            var company = await _companyRepository.GetOneAsync(e => e.Id == companyId && !e.IsDeleted, cancellationToken: cancellationToken);
            if (company is null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Company not found"
                };
            }
            try
            {
                company.DeletedAt = DateTime.UtcNow;
                company.IsDeleted = true;
                _companyRepository.Update(company);
                await _companyRepository.CommitAsync(cancellationToken);
            }
            catch
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while deleting the company."
                };
            }
            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Company deleted successfully"
            };
        }

        private async Task<bool> IsAuthorizedAsync(string userId, int companyId, CancellationToken cancellationToken)
        {
            var member = await _companyMemberRepository.GetOneAsync(m => m.UserId == userId &&
                     m.CompanyId == companyId &&
                     m.IsActive &&
                     (m.Role == CompanyRole.Owner || m.Role == CompanyRole.Admin), tracked: false,
                     cancellationToken: cancellationToken);

            return member != null;
        }
    }
}
