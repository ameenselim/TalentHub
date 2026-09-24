using MediatR;
using Microsoft.Extensions.Logging;
using System;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Application.Interfaces.Services;
using TalentHub.Domain.Entities;
using TalentHub.Domain.Enums.Company;

namespace TalentHub.Application.Features.CompanyImages.Commands.DeleteCompanyImage
{
    public record DeleteCompanyImageCommand(string UserId, int ImageId, int CompanyId) : IRequest<ApiResponse<bool>>;

    public class DeleteCompanyImageCommandHandler : IRequestHandler<DeleteCompanyImageCommand, ApiResponse<bool>>
    {
        private readonly IRepository<CompanyImage> _companyImageRepository;
        private readonly IRepository<CompanyMember> _companyMemberRepository;
        private readonly ICloudinaryServices _cloudinaryServices;
        private readonly ILogger<DeleteCompanyImageCommandHandler> _logger;

        public DeleteCompanyImageCommandHandler(
            IRepository<CompanyImage> companyImageRepository,
            IRepository<CompanyMember> companyMemberRepository,
            ICloudinaryServices cloudinaryServices,
            ILogger<DeleteCompanyImageCommandHandler> logger)
        {
            _companyImageRepository = companyImageRepository;
            _companyMemberRepository = companyMemberRepository;
            _cloudinaryServices = cloudinaryServices;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteCompanyImageCommand command, CancellationToken cancellationToken)
        {
            var userId = command.UserId;
            var imageId = command.ImageId;
            var companyId = command.CompanyId;

            if (!await IsAuthorizedAsync(userId, companyId, cancellationToken))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "You are not authorized to delete images for this company."
                };
            }
            var companyImage = await _companyImageRepository.GetOneAsync(e => e.Id == imageId && e.CompanyId == companyId, cancellationToken: cancellationToken);
            if (companyImage == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Company image not found."
                };
            }
            try
            {
                _companyImageRepository.Delete(companyImage);
                await _companyImageRepository.CommitAsync(cancellationToken);
            }
            catch
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while deleting the image."
                };
            }

            try
            {
                await _cloudinaryServices.DeleteAsync(companyImage.ImagePublicId, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete image {PublicId} from Cloudinary for company {CompanyId}", companyImage.ImagePublicId, companyId);
            }
            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Company image deleted successfully."
            };
        }

        private async Task<bool> IsAuthorizedAsync(string userId, int companyId, CancellationToken cancellationToken)
        {
            var member = await _companyMemberRepository.GetOneAsync(m => m.UserId == userId &&
                        m.CompanyId == companyId && m.IsActive &&
                        (m.Role == CompanyRole.Owner || m.Role == CompanyRole.Admin), tracked: false,
                        cancellationToken: cancellationToken);

            return member != null;
        }
    }
}
