using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Application.Interfaces.Services;
using TalentHub.Domain.Entities;
using TalentHub.Domain.Enums.Company;

namespace TalentHub.Application.Features.Companies.Commands.UpdateCompany
{
    public record UpdateCompanyCommand(string UserId, int CompanyId, UpdateCompanyRequest Request) : IRequest<ApiResponse<CompanyResponse>>;

    public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, ApiResponse<CompanyResponse>>
    {
        private readonly IRepository<Company> _companyRepository;
        private readonly ICloudinaryServices _cloudinaryServices;
        private readonly ILogger<UpdateCompanyCommandHandler> _companyLogger;
        private readonly IRepository<CompanyMember> _companyMemberRepository;

        private const string LogoFolder = "TalentHub/Companies/Logos";
        private const string CoverFolder = "TalentHub/Companies/Covers";

        public UpdateCompanyCommandHandler(
            IRepository<Company> companyRepository,
            ICloudinaryServices cloudinaryServices,
            ILogger<UpdateCompanyCommandHandler> companyLogger,
            IRepository<CompanyMember> companyMemberRepository)
        {
            _companyRepository = companyRepository;
            _cloudinaryServices = cloudinaryServices;
            _companyLogger = companyLogger;
            _companyMemberRepository = companyMemberRepository;
        }

        public async Task<ApiResponse<CompanyResponse>> Handle(UpdateCompanyCommand command, CancellationToken cancellationToken)
        {
            var userId = command.UserId;
            var companyId = command.CompanyId;
            var request = command.Request;

            var company = await _companyRepository.GetOneAsync(e => e.Id == companyId && !e.IsDeleted, cancellationToken: cancellationToken);
            if (company is null)
            {
                return new ApiResponse<CompanyResponse>
                {
                    Success = false,
                    Message = "Company not found"
                };
            }
            if (!await IsAuthorizedAsync(userId, companyId, cancellationToken))
            {
                return new ApiResponse<CompanyResponse>
                {
                    Success = false,
                    Message = "You are not authorized to update this company."
                };
            }
            var name = request.Name.Trim();
            var nameExists = await _companyRepository.GetAsync(e => e.Id != companyId && e.Name == name, tracked: false, cancellationToken: cancellationToken);
            if (nameExists?.Any() == true)
            {
                return new ApiResponse<CompanyResponse>
                {
                    Success = false,
                    Message = "Company with the same name already exists"
                };
            }
            // Keep old images IDs before updating the entity
            var oldLogoPublicId = company.LogoPublicId;
            var oldCoverPublicId = company.CoverImagePublicId;

            // Store new uploaded images IDs, So we can delete them if DB update fails
            string? newLogoPublicId = null;
            string? newCoverPublicId = null;

            try
            {
                company.Name = name;
                company.Description = request.Description?.Trim();
                company.Website = request.Website?.Trim();
                company.Email = request.Email?.Trim();
                company.PhoneNumber = request.PhoneNumber?.Trim();
                company.Country = request.Country?.Trim();
                company.City = request.City?.Trim();
                company.Address = request.Address?.Trim();
                company.FoundedDate = request.FoundedDate;
                company.Size = request.Size?.Trim();
                company.Industry = request.Industry?.Trim();
                company.UpdatedAt = DateTime.UtcNow;
                company.UpdatedBy = userId;

                if (request.Logo is not null)
                {
                    var logoResult = await _cloudinaryServices.UploadImageAsync(request.Logo, LogoFolder, cancellationToken);
                    company.Logo = logoResult.Url;
                    company.LogoPublicId = logoResult.PublicId;
                    newLogoPublicId = logoResult.PublicId;
                }
                if (request.CoverImage is not null)
                {
                    var coverImageResult = await _cloudinaryServices.UploadImageAsync(request.CoverImage, CoverFolder, cancellationToken);
                    company.CoverImage = coverImageResult.Url;
                    company.CoverImagePublicId = coverImageResult.PublicId;
                    newCoverPublicId = coverImageResult.PublicId;
                }

                _companyRepository.Update(company);
                await _companyRepository.CommitAsync(cancellationToken);
            }
            catch
            {
                if (newLogoPublicId != null)
                    await _cloudinaryServices.DeleteAsync(newLogoPublicId, cancellationToken);

                if (newCoverPublicId != null)
                    await _cloudinaryServices.DeleteAsync(newCoverPublicId, cancellationToken);

                return new ApiResponse<CompanyResponse>
                {
                    Success = false,
                    Message = "An error occurred while updating the company."
                };
            }

            await CleanupOldImageAsync(newLogoPublicId, oldLogoPublicId, "logo", companyId);
            await CleanupOldImageAsync(newCoverPublicId, oldCoverPublicId, "cover image", companyId);

            return new ApiResponse<CompanyResponse>
            {
                Success = true,
                Data = MapToResponse(company),
                Message = "Company updated successfully"
            };
        }

        private static CompanyResponse MapToResponse(Company company)
        {
            return new CompanyResponse
            {
                Id = company.Id,
                Name = company.Name,
                Description = company.Description,
                Logo = company.Logo,
                CoverImage = company.CoverImage,
                Website = company.Website,
                Email = company.Email,
                PhoneNumber = company.PhoneNumber,
                Country = company.Country,
                City = company.City,
                Address = company.Address,
                FoundedDate = company.FoundedDate,
                Size = company.Size,
                Industry = company.Industry,
                IsVerified = company.IsVerified
            };
        }

        private async Task CleanupOldImageAsync(string? newPublicId, string? oldPublicId, string imageType, int companyId)
        {
            if (string.IsNullOrWhiteSpace(newPublicId) || string.IsNullOrWhiteSpace(oldPublicId))
                return;

            try
            {
                await _cloudinaryServices.DeleteAsync(oldPublicId, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _companyLogger.LogError(ex, "Failed to delete old {ImageType} {PublicId} for company {CompanyId}", imageType, oldPublicId, companyId);
            }
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
