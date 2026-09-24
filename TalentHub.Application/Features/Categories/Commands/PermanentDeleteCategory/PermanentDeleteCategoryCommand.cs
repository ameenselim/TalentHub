using MediatR;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Application.Interfaces.Services;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.Categories.Commands.PermanentDeleteCategory
{
    public record PermanentDeleteCategoryCommand(int Id) : IRequest<ApiResponse<bool>>;

    public class PermanentDeleteCategoryCommandHandler : IRequestHandler<PermanentDeleteCategoryCommand, ApiResponse<bool>>
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly ICloudinaryServices _cloudinaryServices;

        public PermanentDeleteCategoryCommandHandler(IRepository<Category> categoryRepository, ICloudinaryServices cloudinaryServices)
        {
            _categoryRepository = categoryRepository;
            _cloudinaryServices = cloudinaryServices;
        }

        public async Task<ApiResponse<bool>> Handle(PermanentDeleteCategoryCommand command, CancellationToken cancellationToken)
        {
            var id = command.Id;
            var category = await _categoryRepository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken);
            if (category is null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Category not found"
                };
            }
            var publicId = category.IconPublicId;
            try
            {
                _categoryRepository.Delete(category);
                await _categoryRepository.CommitAsync(cancellationToken);

                // If the category had an associated image, delete it from Cloudinary
                if (!string.IsNullOrWhiteSpace(publicId))
                {
                    await _cloudinaryServices.DeleteAsync(publicId, cancellationToken);
                }
            }
            catch
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while permanently deleting the category."
                };
            }

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Category permanently deleted successfully"
            };
        }
    }
}
