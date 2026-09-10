using System;
using System.Collections.Generic;
using System.Text;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Application.Interfaces.Services;

namespace TalentHub.Infrastructure.Services
{
    public class CategoryServices : ICategoryServices
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly ICloudinaryServices _cloudinaryServices;

        public CategoryServices(IRepository<Category> categoryRepository, ICloudinaryServices cloudinaryServices)
        {
            _categoryRepository = categoryRepository;
            _cloudinaryServices = cloudinaryServices;
        }
        public async Task<ApiResponse<IEnumerable<CategoryResponse>>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAsync(e => !e.IsDeleted, tracked: false);
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
        public async Task<ApiResponse<CategoryResponse>> GetCategoryByIdAsync(int categoryId)
        {
            var category = await _categoryRepository.GetOneAsync(e => e.Id == categoryId && !e.IsDeleted, tracked: false);
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
        public async Task<ApiResponse<CategoryResponse>> CreateAsync(string userId, CreateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            var name = request.Name.Trim();

            var existingCategory = await _categoryRepository.GetAsync(
                e => e.Name.ToLower() == name.ToLower(),tracked: false,
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
        public async Task<ApiResponse<CategoryResponse>> UpdateAsync(string userId, int id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
        {
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
                e => e.Name.ToLower() == name.ToLower() && e.Id != id,cancellationToken: cancellationToken);

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
        public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var category = await _categoryRepository.GetOneAsync(e => e.Id == id && !e.IsDeleted,cancellationToken: cancellationToken);
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
        public async Task<ApiResponse<bool>> PermanentDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var category = await _categoryRepository.GetOneAsync(e => e.Id == id,cancellationToken: cancellationToken);
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
