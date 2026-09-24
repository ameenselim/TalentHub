using MediatR;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.Categories.Queries.GetCategoryById
{
    public record GetCategoryByIdQuery(int CategoryId) : IRequest<ApiResponse<CategoryResponse>>;

    public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, ApiResponse<CategoryResponse>>
    {
        private readonly IRepository<Category> _categoryRepository;

        public GetCategoryByIdQueryHandler(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<ApiResponse<CategoryResponse>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetOneAsync(e => e.Id == request.CategoryId && !e.IsDeleted, tracked: false, cancellationToken: cancellationToken);
            if (category is null)
            {
                return new ApiResponse<CategoryResponse>()
                {
                    Success = false,
                    Message = "Category not found",
                };
            }

            return new ApiResponse<CategoryResponse>()
            {
                Success = true,
                Data = new CategoryResponse
                {
                    Id = category.Id,
                    Name = category.Name,
                    Icon = category.Icon
                },
                Message = "Category retrieved successfully"
            };
        }
    }
}
