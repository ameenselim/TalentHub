using MediatR;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.Skills.Commands.UpdateSkill
{
    public record UpdateSkillCommand(string UserId, int Id, UpdateSkillRequest Request) : IRequest<ApiResponse<SkillResponse>>;

    public class UpdateSkillCommandHandler : IRequestHandler<UpdateSkillCommand, ApiResponse<SkillResponse>>
    {
        private readonly IRepository<Skill> _skillRepository;

        public UpdateSkillCommandHandler(IRepository<Skill> skillRepository)
        {
            _skillRepository = skillRepository;
        }

        public async Task<ApiResponse<SkillResponse>> Handle(UpdateSkillCommand command, CancellationToken cancellationToken)
        {
            var userId = command.UserId;
            var id = command.Id;
            var request = command.Request;

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
                e => e.Name.ToLower() == name.ToLower() && e.Id != id, cancellationToken: cancellationToken);

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
    }
}
