using MediatR;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.Skills.Commands.CreateSkill
{
    public record CreateSkillCommand(string UserId, CreateSkillRequest Request) : IRequest<ApiResponse<SkillResponse>>;

    public class CreateSkillCommandHandler : IRequestHandler<CreateSkillCommand, ApiResponse<SkillResponse>>
    {
        private readonly IRepository<Skill> _skillRepository;

        public CreateSkillCommandHandler(IRepository<Skill> skillRepository)
        {
            _skillRepository = skillRepository;
        }

        public async Task<ApiResponse<SkillResponse>> Handle(CreateSkillCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var userId = command.UserId;
            var name = request.Name.Trim();

            var existingSkill = await _skillRepository.GetAsync(
                e => e.Name.ToLower() == name.ToLower(), tracked: false,
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
    }
}
