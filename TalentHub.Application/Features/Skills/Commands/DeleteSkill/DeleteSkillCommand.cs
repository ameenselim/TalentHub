using MediatR;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.Skills.Commands.DeleteSkill
{
    public record DeleteSkillCommand(int Id) : IRequest<ApiResponse<bool>>;

    public class DeleteSkillCommandHandler : IRequestHandler<DeleteSkillCommand, ApiResponse<bool>>
    {
        private readonly IRepository<Skill> _skillRepository;

        public DeleteSkillCommandHandler(IRepository<Skill> skillRepository)
        {
            _skillRepository = skillRepository;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteSkillCommand command, CancellationToken cancellationToken)
        {
            var id = command.Id;
            var skill = await _skillRepository.GetOneAsync(e => e.Id == id && !e.IsDeleted, cancellationToken: cancellationToken);
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
    }
}
