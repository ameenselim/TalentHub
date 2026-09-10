using System;
using System.Collections.Generic;
using System.Text;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;

namespace TalentHub.Application.Interfaces.Services
{
    public interface ICategoryServices
    {
        Task<ApiResponse<IEnumerable<CategoryResponse>>> GetAllCategoriesAsync();
        Task<ApiResponse<CategoryResponse>> GetCategoryByIdAsync(int categoryId);
        Task<ApiResponse<CategoryResponse>> CreateAsync(string userId, CreateCategoryRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<CategoryResponse>> UpdateAsync(string userId, int id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> PermanentDeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
