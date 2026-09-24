using MediatR;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.Skills.Commands.PermanentDeleteSkill
{
    public record PermanentDeleteSkillCommand(int Id) : IRequest<ApiResponse<bool>>;

    public class PermanentDeleteSkillCommandHandler : IRequestHandler<PermanentDeleteSkillCommand, ApiResponse<bool>>
    {
        private readonly IRepository<Skill> _skillRepository;

        public PermanentDeleteSkillCommandHandler(IRepository<Skill> skillRepository)
        {
            _skillRepository = skillRepository;
        }

        public async Task<ApiResponse<bool>> Handle(PermanentDeleteSkillCommand command, CancellationToken cancellationToken)
        {
            var id = command.Id;
            var skill = await _skillRepository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken);
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
