using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq.Expressions;
using System.Text;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Application.Interfaces.Services;
using TalentHub.Domain.Enums.Company;
using TalentHub.Infrastructure.Extensions;

namespace TalentHub.Infrastructure.Services
{
    public class CompanyServices : ICompanyServices
    {
        private readonly IRepository<Company> _companyRepository;
        private readonly ICloudinaryServices _cloudinaryServices;
        private readonly ILogger<CompanyServices> _companyLogger;
        private readonly IRepository<CompanyMember> _companyMemberRepository;

        private const string LogoFolder = "TalentHub/Companies/Logos";
        private const string CoverFolder = "TalentHub/Companies/Covers";

        public CompanyServices(IRepository<Company> companyRepository, ICloudinaryServices cloudinaryServices ,ILogger<CompanyServices> companyLogger, IRepository<CompanyMember> companyMemberRepository)
        {
            _companyRepository = companyRepository;
            _cloudinaryServices = cloudinaryServices;
            _companyLogger = companyLogger;
            _companyMemberRepository = companyMemberRepository;
        }
        public async Task<ApiResponse<PagedResponse<CompanyListResponse>>> GetAllCompaniesAsync(CompanyFilterRequest companyRequest ,CancellationToken cancellationToken = default)
        {
            Expression<Func<Company, bool>> filter = e => !e.IsDeleted;
            if(!string.IsNullOrWhiteSpace(companyRequest.Name))
            {
                filter = filter.And(e =>EF.Functions.Like(e.Name,$"%{companyRequest.Name.Trim()}%"));
            }
            if(!string.IsNullOrEmpty(companyRequest.Country))
            {
                filter = filter.And(e => e.Country == companyRequest.Country);
            }
            if(!string.IsNullOrEmpty(companyRequest.City))
            {
                filter = filter.And(e => e.City == companyRequest.City);
            }
            if(!string.IsNullOrEmpty(companyRequest.Industry))
            {
                filter = filter.And(e => e.Industry == companyRequest.Industry);
            }
            if(!string.IsNullOrEmpty(companyRequest.Size))
            {
                filter = filter.And(e => e.Size == companyRequest.Size);
            }
            if(companyRequest.IsVerified.HasValue)
            {
                filter = filter.And(e => e.IsVerified == companyRequest.IsVerified.Value);
            }

            int totalCount = await _companyRepository.CountAsync(filter, cancellationToken);
            int totalPages = (int)Math.Ceiling((double)totalCount / companyRequest.PageSize);

            var companies = await _companyRepository.GetAsync(
                expression: filter,
                pageSize: companyRequest.PageSize,
                page: companyRequest.PageNumber,
                tracked: false,
                cancellationToken: cancellationToken,
                include:q=>q.Include(c=>c.Followers).Include(c=>c.Jobs)
                );
            var pagedData = new PagedResponse<CompanyListResponse>
            {
                Data = companies.Adapt<IEnumerable<CompanyListResponse>>(),
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageSize = companyRequest.PageSize,
                PageNumber = companyRequest.PageNumber
            };

            return new ApiResponse<PagedResponse<CompanyListResponse>>
            {
                Success = true,
                Message = "Companies retrieved successfully",
                Data = pagedData
            };
        }
        public async Task<ApiResponse<CompanyResponse>> GetCompanyByIdAsync(int companyId, CancellationToken cancellationToken = default)
        {
            var company = await _companyRepository.GetOneAsync(e => e.Id == companyId && !e.IsDeleted, tracked: false, cancellationToken: cancellationToken);
            if (company is null)
            {
                return new ApiResponse<CompanyResponse>
                {
                    Success = false,
                    Message = "Company not found"
                };
            }

            return new ApiResponse<CompanyResponse>
            {
                Success = true,
                Data = MapToResponse(company),
                Message = "Company retrieved successfully"
            };
        }
        public async Task<ApiResponse<CompanyResponse>> UpdateAsync(string userId, int companyId, UpdateCompanyRequest request, CancellationToken cancellationToken = default)
        {

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
        public async Task<ApiResponse<bool>> DeleteAsync(string userId, int companyId, CancellationToken cancellationToken = default)
        {
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
            if (!await IsAuthorizedAsync(userId, companyId, cancellationToken))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "You are not authorized to delete this company."
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
        private CompanyResponse MapToResponse(Company company)
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
               _companyLogger.LogError(ex, "Failed to delete old {ImageType} {PublicId} for company {CompanyId}",imageType, oldPublicId, companyId);
            }
        }
        private async Task<bool> IsAuthorizedAsync(string userId, int companyId, CancellationToken cancellationToken)
        {
            var member = await _companyMemberRepository.GetOneAsync(m => m.UserId == userId &&
                     m.CompanyId == companyId &&
                     m.IsActive &&
                     (m.Role == CompanyRole.Owner || m.Role == CompanyRole.Admin),tracked: false,
                     cancellationToken: cancellationToken);

            return member != null;
        }
    }
}
