using MediatR;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Domain.Entities;
using TalentHub.Domain.Enums.Company;

namespace TalentHub.Application.Features.CompanyImages.Commands.UpdateCompanyImage
{
    public record UpdateCompanyImageCommand(string UserId, int ImageId, int CompanyId, UpdateCompanyImageRequest Request) : IRequest<ApiResponse<CompanyImagesResponse>>;

    public class UpdateCompanyImageCommandHandler : IRequestHandler<UpdateCompanyImageCommand, ApiResponse<CompanyImagesResponse>>
    {
        private readonly IRepository<CompanyImage> _companyImageRepository;
        private readonly IRepository<CompanyMember> _companyMemberRepository;

        public UpdateCompanyImageCommandHandler(
            IRepository<CompanyImage> companyImageRepository,
            IRepository<CompanyMember> companyMemberRepository)
        {
            _companyImageRepository = companyImageRepository;
            _companyMemberRepository = companyMemberRepository;
        }

        public async Task<ApiResponse<CompanyImagesResponse>> Handle(UpdateCompanyImageCommand command, CancellationToken cancellationToken)
        {
            var userId = command.UserId;
            var imageId = command.ImageId;
            var companyId = command.CompanyId;
            var request = command.Request;

            if (!await IsAuthorizedAsync(userId, companyId, cancellationToken))
            {
                return new ApiResponse<CompanyImagesResponse>
                {
                    Success = false,
                    Message = "You are not authorized to update images for this company."
                };
            }
            var companyImage = await _companyImageRepository.GetOneAsync(e => e.Id == imageId && e.CompanyId == companyId, cancellationToken: cancellationToken);
            if (companyImage == null)
            {
                return new ApiResponse<CompanyImagesResponse>
                {
                    Success = false,
                    Message = "Company image not found."
                };
            }
            if (request.IsCover && !companyImage.IsCover)
            {
                await ClearExistingCoverAsync(companyId, cancellationToken);
            }
            companyImage.Caption = request.Caption?.Trim();
            companyImage.IsCover = request.IsCover;

            _companyImageRepository.Update(companyImage);
            await _companyImageRepository.CommitAsync(cancellationToken);

            return new ApiResponse<CompanyImagesResponse>
            {
                Success = true,
                Data = MapToResponse(companyImage),
                Message = "Company image updated successfully."
            };
        }

        private static CompanyImagesResponse MapToResponse(CompanyImage image)
        {
            return new CompanyImagesResponse
            {
                CompanyId = image.CompanyId,
                ImageUrl = image.ImageUrl,
                IsCover = image.IsCover,
                Caption = image.Caption
            };
        }

        private async Task ClearExistingCoverAsync(int companyId, CancellationToken cancellationToken)
        {
            var existingCover = await _companyImageRepository.GetOneAsync(e => e.CompanyId == companyId && e.IsCover, cancellationToken: cancellationToken);
            if (existingCover != null)
            {
                existingCover.IsCover = false;
                _companyImageRepository.Update(existingCover);
                await _companyImageRepository.CommitAsync(cancellationToken);
            }
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
