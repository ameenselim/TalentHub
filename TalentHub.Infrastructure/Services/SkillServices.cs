using System;
using System.Collections.Generic;
using System.Text;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Application.Interfaces.Services;

namespace TalentHub.Infrastructure.Services
{
    public class SkillServices : ISkillServices
    {
        private readonly IRepository<Skill> _skillRepository;
        private readonly ICloudinaryServices _cloudinaryServices;

        public SkillServices(IRepository<Skill> skillRepository, ICloudinaryServices cloudinaryServices)
        {
            _skillRepository = skillRepository;
            _cloudinaryServices = cloudinaryServices;
        }
        public async Task<ApiResponse<IEnumerable<SkillResponse>>> GetAllSkillsAsync()
        {
            var skills = await _skillRepository.GetAsync(e => !e.IsDeleted, tracked: false);
            if (skills is null || !skills.Any())
            {
                return new ApiResponse<IEnumerable<SkillResponse>>()
                {
                    Success = false,
                    Message = "No skills found",
                };
            }
            return new ApiResponse<IEnumerable<SkillResponse>>()
            {
                Success = true,
                Data = skills.Select(c => new SkillResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                }),
                Message = "Skills retrieved successfully"
            };
        }
        public async Task<ApiResponse<SkillResponse>> GetSkillByIdAsync(int skillId)
        {
            var skill = await _skillRepository.GetOneAsync(e => e.Id == skillId && !e.IsDeleted, tracked: false);
            if (skill is null)
            {
                return new ApiResponse<SkillResponse>()
                {
                    Success = false,
                    Message = "Skill not found",
                };
            }
            return new ApiResponse<SkillResponse>()
            {
                Success = true,
                Data = new SkillResponse
                {
                    Id = skill.Id,
                    Name = skill.Name
                },
                Message = "Skill retrieved successfully"
            };
        }
        public async Task<ApiResponse<SkillResponse>> CreateAsync(string userId, CreateSkillRequest request, CancellationToken cancellationToken = default)
        {
            var name = request.Name.Trim();

            var existingSkill = await _skillRepository.GetAsync(
                e => e.Name.ToLower() == name.ToLower(),tracked: false,
                cancellationToken: cancellationToken);

            if (existingSkill?.Any() == true)
            {
                return new ApiResponse<SkillResponse>
                {
                    Success = false,
                    Message = "Skill with the same name already exists",
                };
            }
            var skill = new Skill
            {
                Name = name,
                CreatedBy = userId,
            };
            try
            {       
                await _skillRepository.CreateAsync(skill, cancellationToken);
                await _skillRepository.CommitAsync(cancellationToken);
            }
            catch
            {
                return new ApiResponse<SkillResponse>
                {
                    Success = false,
                    Message = "An error occurred while creating the skill."
                };
            }

            return new ApiResponse<SkillResponse>
            {
                Success = true,
                Data = new SkillResponse
                {
                    Id = skill.Id,
                    Name = skill.Name
                },
                Message = "Skill created successfully"
            };
        }
        public async Task<ApiResponse<SkillResponse>> UpdateAsync(string userId, int id, UpdateSkillRequest request, CancellationToken cancellationToken = default)
        {
            var skill = await _skillRepository.GetOneAsync(e => e.Id == id && !e.IsDeleted, cancellationToken: cancellationToken);
            if (skill is null)
            {
                return new ApiResponse<SkillResponse>
                {
                    Success = false,
                    Message = "Skill not found"
                };
            }
            var name = request.Name.Trim();

            // Check if another skill with the same name exists (excluding the current skill)
            var existingSkill = await _skillRepository.GetAsync(
                e => e.Name.ToLower() == name.ToLower() && e.Id != id,cancellationToken: cancellationToken);

            if (existingSkill?.Any() == true)
            {
                return new ApiResponse<SkillResponse>
                {
                    Success = false,
                    Message = "Skill with the same name already exists"
                };
            }
            try
            {
                skill.Name = name;
                skill.UpdatedBy = userId;
                skill.UpdatedAt = DateTime.UtcNow;
                _skillRepository.Update(skill);
                await _skillRepository.CommitAsync(cancellationToken);
            }
            catch
            {
                return new ApiResponse<SkillResponse>
                {
                    Success = false,
                    Message = "An error occurred while updating the skill."
                };
            }

            return new ApiResponse<SkillResponse>
            {
                Success = true,
                Data = new SkillResponse
                {
                    Id = skill.Id,
                    Name = skill.Name
                },
                Message = "Skill updated successfully"
            };
        }
        public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var skill = await _skillRepository.GetOneAsync(e => e.Id == id && !e.IsDeleted,cancellationToken: cancellationToken);
            if (skill is null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Skill not found"
                };
            }
            try
            {
                skill.IsDeleted = true;
                skill.DeletedAt = DateTime.UtcNow;
                _skillRepository.Update(skill);
                await _skillRepository.CommitAsync(cancellationToken);
            }
            catch
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while deleting the skill."
                };
            }
            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Skill deleted successfully"
            };
        }
        public async Task<ApiResponse<bool>> PermanentDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var skill = await _skillRepository.GetOneAsync(e => e.Id == id,cancellationToken: cancellationToken);
            if (skill is null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Skill not found"
                };
            }
            try
            {
                _skillRepository.Delete(skill);
                await _skillRepository.CommitAsync(cancellationToken);
            }
            catch
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while permanently deleting the skill."
                };
            }
            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Skill permanently deleted successfully"
            };
        }
    }
}
