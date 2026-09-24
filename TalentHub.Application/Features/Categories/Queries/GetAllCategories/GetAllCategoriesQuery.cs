using MediatR;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.Categories.Queries.GetAllCategories
{
    public record GetAllCategoriesQuery : IRequest<ApiResponse<IEnumerable<CategoryResponse>>>;

    public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, ApiResponse<IEnumerable<CategoryResponse>>>
    {
        private readonly IRepository<Category> _categoryRepository;

        public GetAllCategoriesQueryHandler(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<ApiResponse<IEnumerable<CategoryResponse>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _categoryRepository.GetAsync(e => !e.IsDeleted, tracked: false, cancellationToken: cancellationToken);
            if (categories is null || !categories.Any())
            {
                return new ApiResponse<IEnumerable<CategoryResponse>>()
                {
                    Success = false,
                    Message = "No categories found",
                };
            }

            return new ApiResponse<IEnumerable<CategoryResponse>>()
            {
                Success = true,
                Data = categories.Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Icon = c.Icon
                }),
                Message = "Categories retrieved successfully"
            };
        }
    }
}
