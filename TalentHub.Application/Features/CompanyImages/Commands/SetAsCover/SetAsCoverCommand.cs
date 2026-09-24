using MediatR;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Domain.Entities;
using TalentHub.Domain.Enums.Company;

namespace TalentHub.Application.Features.CompanyImages.Commands.SetAsCover
{
    public record SetAsCoverCommand(string UserId, int CompanyId, int ImageId) : IRequest<ApiResponse<bool>>;

    public class SetAsCoverCommandHandler : IRequestHandler<SetAsCoverCommand, ApiResponse<bool>>
    {
        private readonly IRepository<CompanyImage> _companyImageRepository;
        private readonly IRepository<CompanyMember> _companyMemberRepository;

        public SetAsCoverCommandHandler(
            IRepository<CompanyImage> companyImageRepository,
            IRepository<CompanyMember> companyMemberRepository)
        {
            _companyImageRepository = companyImageRepository;
            _companyMemberRepository = companyMemberRepository;
        }

        public async Task<ApiResponse<bool>> Handle(SetAsCoverCommand command, CancellationToken cancellationToken)
        {
            var userId = command.UserId;
            var companyId = command.CompanyId;
            var imageId = command.ImageId;

            if (!await IsAuthorizedAsync(userId, companyId, cancellationToken))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "You are not authorized to set cover images for this company."
                };
            }
            var image = await _companyImageRepository.GetOneAsync(e => e.Id == imageId && e.CompanyId == companyId, cancellationToken: cancellationToken);
            if (image is null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Image not found"
                };
            }
            if (image.IsCover)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Image is already the cover"
                };
            }
            try
            {
                await ClearExistingCoverAsync(companyId, cancellationToken);

                image.IsCover = true;
                _companyImageRepository.Update(image);
                await _companyImageRepository.CommitAsync(cancellationToken);
            }
            catch
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while setting the cover image."
                };
            }

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Image set as cover successfully."
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
