using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Application.Interfaces.Services;
using TalentHub.Domain.Enums.Company;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TalentHub.Infrastructure.Services
{
    public class CompanyImageServices : ICompanyImageServices
    {
        public readonly IRepository<CompanyImage> _companyImageRepository;
        public readonly IRepository<CompanyMember> _companyMemberRepository;
        public readonly IRepository<Company> _companyRepository;
        private readonly ICloudinaryServices _cloudinaryServices;
        private readonly ILogger<CompanyImageServices> _logger;
        private const string ImageFolder = "company_images";
        private const int MaxImagesPerCompany = 5;

        public CompanyImageServices(IRepository<CompanyImage> companyImageRepository, ILogger<CompanyImageServices> logger, IRepository<Company> companyRepository, IRepository<CompanyMember> companyMemberRepository, ICloudinaryServices cloudinaryServices)
        {
            _companyImageRepository = companyImageRepository;
            _cloudinaryServices = cloudinaryServices;
            _companyRepository = companyRepository;
            _companyMemberRepository = companyMemberRepository;
            _logger = logger;
        }
        public async Task<ApiResponse<IEnumerable<CompanyImagesResponse>>> GetCompanyImagesAsync(int companyId, CancellationToken cancellationToken = default)
        {
            var companyImages = await _companyImageRepository.GetAsync(e => e.CompanyId == companyId, tracked: false,
                cancellationToken: cancellationToken);

            return new ApiResponse<IEnumerable<CompanyImagesResponse>>
            {
                Success = true,
                Data = companyImages?.Select(MapToResponse) ?? Enumerable.Empty<CompanyImagesResponse>(),
                Message = "Company images retrieved successfully."
            };
        }
        public async Task<ApiResponse<CompanyImagesResponse>> GetCompanyImageByIdAsync(int imageId, int companyId, CancellationToken cancellationToken = default)
        {
            var companyImage = await _companyImageRepository.GetOneAsync(e => e.Id == imageId && e.CompanyId == companyId, cancellationToken: cancellationToken);
            if (companyImage == null)
            {
                return new ApiResponse<CompanyImagesResponse>
                {
                    Success = false,
                    Message = "Company image not found."
                };
            }
            return new ApiResponse<CompanyImagesResponse>
            {
                Success = true,
                Data = MapToResponse(companyImage),
                Message = "Company image retrieved successfully."
            };
        }
        public async Task<ApiResponse<CompanyImagesResponse>> AddImageAsync(string userId, int companyId, AddCompanyImageRequest request, CancellationToken cancellationToken = default)
        {
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

        // Update the caption and cover status of an existing company image
        public async Task<ApiResponse<CompanyImagesResponse>> UpdateImageAsync(string userId, int imageId, int companyId, UpdateCompanyImageRequest request, CancellationToken cancellationToken = default)
        {
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




        public async Task<ApiResponse<bool>> DeleteImageAsync(string userId, int imageId, int companyId, CancellationToken cancellationToken = default)
        {
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
                // احذف من الـ DB أولاً
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
            // ثم احذف من Cloudinary
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

        public async Task<ApiResponse<bool>> SetAsCoverAsync(string userId, int companyId, int imageId, CancellationToken cancellationToken = default)
        {
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

        private CompanyImagesResponse MapToResponse(CompanyImage image)
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
