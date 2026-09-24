using MediatR;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Application.Interfaces.Services;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.Categories.Commands.UpdateCategory
{
    public record UpdateCategoryCommand(string UserId, int Id, UpdateCategoryRequest Request) : IRequest<ApiResponse<CategoryResponse>>;

    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, ApiResponse<CategoryResponse>>
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly ICloudinaryServices _cloudinaryServices;

        public UpdateCategoryCommandHandler(IRepository<Category> categoryRepository, ICloudinaryServices cloudinaryServices)
        {
            _categoryRepository = categoryRepository;
            _cloudinaryServices = cloudinaryServices;
        }

        public async Task<ApiResponse<CategoryResponse>> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
        {
            var userId = command.UserId;
            var id = command.Id;
            var request = command.Request;

            var category = await _categoryRepository.GetOneAsync(e => e.Id == id && !e.IsDeleted, cancellationToken: cancellationToken);
            if (category is null)
            {
                return new ApiResponse<CategoryResponse>
                {
                    Success = false,
                    Message = "Category not found"
                };
            }
            var name = request.Name.Trim();

            // Check if another category with the same name exists (excluding the current category)
            var existingCategory = await _categoryRepository.GetAsync(
                e => e.Name.ToLower() == name.ToLower() && e.Id != id, cancellationToken: cancellationToken);

            if (existingCategory?.Any() == true)
            {
                return new ApiResponse<CategoryResponse>
                {
                    Success = false,
                    Message = "Category with the same name already exists"
                };
            }
            var oldPublicId = category.IconPublicId;
            string? newUploadedPublicId = null;

            try
            {
                category.Name = name;
                category.UpdatedBy = userId;
                category.UpdatedAt = DateTime.UtcNow;

                if (request.Icon != null)
                {
                    var uploadResult = await _cloudinaryServices.UploadImageAsync(request.Icon, "TalentHub/Categories", cancellationToken);
                    category.Icon = uploadResult.Url;
                    category.IconPublicId = uploadResult.PublicId;
                    newUploadedPublicId = uploadResult.PublicId;
                }

                _categoryRepository.Update(category);
                await _categoryRepository.CommitAsync(cancellationToken);

                // If a new image was uploaded and the old image exists, delete the old image from Cloudinary
                if (newUploadedPublicId != null && !string.IsNullOrWhiteSpace(oldPublicId))
                {
                    await _cloudinaryServices.DeleteAsync(oldPublicId, cancellationToken);
                }
            }
            catch
            {
                // If a new image was uploaded but the update failed, delete the newly uploaded image from Cloudinary
                if (newUploadedPublicId != null)
                {
                    await _cloudinaryServices.DeleteAsync(newUploadedPublicId, cancellationToken);
                }

                return new ApiResponse<CategoryResponse>
                {
                    Success = false,
                    Message = "An error occurred while updating the category."
                };
            }

            return new ApiResponse<CategoryResponse>
            {
                Success = true,
                Data = new CategoryResponse
                {
                    Id = category.Id,
                    Name = category.Name,
                    Icon = category.Icon
                },
                Message = "Category updated successfully"
            };
        }
    }
}
