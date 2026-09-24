using MediatR;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.Skills.Queries.GetAllSkills
{
    public record GetAllSkillsQuery : IRequest<ApiResponse<IEnumerable<SkillResponse>>>;

    public class GetAllSkillsQueryHandler : IRequestHandler<GetAllSkillsQuery, ApiResponse<IEnumerable<SkillResponse>>>
    {
        private readonly IRepository<Skill> _skillRepository;

        public GetAllSkillsQueryHandler(IRepository<Skill> skillRepository)
        {
            _skillRepository = skillRepository;
        }

        public async Task<ApiResponse<IEnumerable<SkillResponse>>> Handle(GetAllSkillsQuery request, CancellationToken cancellationToken)
        {
            var skills = await _skillRepository.GetAsync(e => !e.IsDeleted, tracked: false, cancellationToken: cancellationToken);
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
    }
}
