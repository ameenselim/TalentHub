using MediatR;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.Skills.Queries.GetSkillById
{
    public record GetSkillByIdQuery(int SkillId) : IRequest<ApiResponse<SkillResponse>>;

    public class GetSkillByIdQueryHandler : IRequestHandler<GetSkillByIdQuery, ApiResponse<SkillResponse>>
    {
        private readonly IRepository<Skill> _skillRepository;

        public GetSkillByIdQueryHandler(IRepository<Skill> skillRepository)
        {
            _skillRepository = skillRepository;
        }

        public async Task<ApiResponse<SkillResponse>> Handle(GetSkillByIdQuery request, CancellationToken cancellationToken)
        {
            var skill = await _skillRepository.GetOneAsync(e => e.Id == request.SkillId && !e.IsDeleted, tracked: false, cancellationToken: cancellationToken);
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
    }
}
