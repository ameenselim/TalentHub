using MediatR;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.Categories.Commands.DeleteCategory
{
    public record DeleteCategoryCommand(int Id) : IRequest<ApiResponse<bool>>;

    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, ApiResponse<bool>>
    {
        private readonly IRepository<Category> _categoryRepository;

        public DeleteCategoryCommandHandler(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
        {
            var id = command.Id;
            var category = await _categoryRepository.GetOneAsync(e => e.Id == id && !e.IsDeleted, cancellationToken: cancellationToken);
            if (category is null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Category not found"
                };
            }
            try
            {
                category.IsDeleted = true;
                category.DeletedAt = DateTime.UtcNow;
                _categoryRepository.Update(category);
                await _categoryRepository.CommitAsync(cancellationToken);
            }
            catch
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while deleting the category."
                };
            }
            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Category deleted successfully"
            };
        }
    }
}
