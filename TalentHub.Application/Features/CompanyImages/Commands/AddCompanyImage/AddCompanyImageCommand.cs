using MediatR;
using System;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Application.Interfaces.Services;
using TalentHub.Domain.Entities;
using TalentHub.Domain.Enums.Company;

namespace TalentHub.Application.Features.CompanyImages.Commands.AddCompanyImage
{
    public record AddCompanyImageCommand(string UserId, int CompanyId, AddCompanyImageRequest Request) : IRequest<ApiResponse<CompanyImagesResponse>>;

    public class AddCompanyImageCommandHandler : IRequestHandler<AddCompanyImageCommand, ApiResponse<CompanyImagesResponse>>
    {
        private readonly IRepository<CompanyImage> _companyImageRepository;
        private readonly IRepository<CompanyMember> _companyMemberRepository;
        private readonly IRepository<Company> _companyRepository;
        private readonly ICloudinaryServices _cloudinaryServices;

        private const string ImageFolder = "company_images";

        public AddCompanyImageCommandHandler(
            IRepository<CompanyImage> companyImageRepository,
            IRepository<CompanyMember> companyMemberRepository,
            IRepository<Company> companyRepository,
            ICloudinaryServices cloudinaryServices)
        {
            _companyImageRepository = companyImageRepository;
            _companyMemberRepository = companyMemberRepository;
            _companyRepository = companyRepository;
            _cloudinaryServices = cloudinaryServices;
        }

        public async Task<ApiResponse<CompanyImagesResponse>> Handle(AddCompanyImageCommand command, CancellationToken cancellationToken)
        {
            var userId = command.UserId;
            var companyId = command.CompanyId;
            var request = command.Request;

            if (!await IsAuthorizedAsync(userId, companyId, cancellationToken))
            {
                return new ApiResponse<CompanyImagesResponse>
                {
                    Success = false,
                    Message = "You are not authorized to add images for this company."
                };
            }
            if (request.Image is null)
            {
                return new ApiResponse<CompanyImagesResponse>
                {
                    Success = false,
                    Message = "Image is required"
                };
            }
            string? uploadedPublicId = null;

            try
            {
                var uploadResult = await _cloudinaryServices.UploadImageAsync(request.Image, ImageFolder, cancellationToken);
                uploadedPublicId = uploadResult.PublicId;

                // check if the cover image is being added, if yes, clear the existing cover image for the company
                if (request.IsCover)
                {
                    await ClearExistingCoverAsync(companyId, cancellationToken);
                }

                var companyImage = new CompanyImage
                {
                    CompanyId = companyId,
                    ImageUrl = uploadResult.Url,
                    ImagePublicId = uploadResult.PublicId,
                    Caption = request.Caption?.Trim(),
                    IsCover = request.IsCover
                };

                await _companyImageRepository.CreateAsync(companyImage, cancellationToken);
                await _companyImageRepository.CommitAsync(cancellationToken);

                return new ApiResponse<CompanyImagesResponse>
                {
                    Success = true,
                    Data = MapToResponse(companyImage)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                if (uploadedPublicId != null)
                    await _cloudinaryServices.DeleteAsync(uploadedPublicId, cancellationToken);

                return new ApiResponse<CompanyImagesResponse>
                {
                    Success = false,
                    Message = "An error occurred while adding the image."
                };
            }
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
