using MediatR;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Application.Interfaces.Services;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.Categories.Commands.CreateCategory
{
    public record CreateCategoryCommand(string UserId, CreateCategoryRequest Request) : IRequest<ApiResponse<CategoryResponse>>;

    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, ApiResponse<CategoryResponse>>
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly ICloudinaryServices _cloudinaryServices;

        public CreateCategoryCommandHandler(IRepository<Category> categoryRepository, ICloudinaryServices cloudinaryServices)
        {
            _categoryRepository = categoryRepository;
            _cloudinaryServices = cloudinaryServices;
        }

        public async Task<ApiResponse<CategoryResponse>> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var userId = command.UserId;
            var name = request.Name.Trim();

            var existingCategory = await _categoryRepository.GetAsync(
                e => e.Name.ToLower() == name.ToLower(), tracked: false,
                cancellationToken: cancellationToken);

            if (existingCategory?.Any() == true)
            {
                return new ApiResponse<CategoryResponse>
                {
                    Success = false,
                    Message = "Category with the same name already exists",
                };
            }

            var category = new Category
            {
                Name = name,
                CreatedBy = userId,
            };
            string? uploadedPublicId = null;

            try
            {
                if (request.Icon != null)
                {
                    var uploadResult = await _cloudinaryServices.UploadImageAsync(request.Icon, "TalentHub/Categories", cancellationToken);
                    category.Icon = uploadResult.Url;
                    category.IconPublicId = uploadResult.PublicId;
                    uploadedPublicId = uploadResult.PublicId;
                }
                await _categoryRepository.CreateAsync(category, cancellationToken);
                await _categoryRepository.CommitAsync(cancellationToken);
            }
            catch
            {
                if (uploadedPublicId != null)
                    await _cloudinaryServices.DeleteAsync(uploadedPublicId, cancellationToken);

                return new ApiResponse<CategoryResponse>
                {
                    Success = false,
                    Message = "An error occurred while creating the category."
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
                Message = "Category created successfully"
            };
        }
    }
}
