using MediatR;
using Microsoft.EntityFrameworkCore;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Features.Resumes.Queries.GetAllResumesByUserId;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.Resumes.Queries.GetResumeById
{
    public record GetResumeByIdQuery(int ResumeId) : IRequest<ApiResponse<ResumeResponse>>;

    public class GetResumeByIdQueryHandler : IRequestHandler<GetResumeByIdQuery, ApiResponse<ResumeResponse>>
    {
        private readonly IRepository<Resume> _resumeRepository;

        public GetResumeByIdQueryHandler(IRepository<Resume> resumeRepository)
        {
            _resumeRepository = resumeRepository;
        }

        public async Task<ApiResponse<ResumeResponse>> Handle(GetResumeByIdQuery query, CancellationToken cancellationToken)
        {
            var resume = await _resumeRepository.GetOneAsync(
                e => e.Id == query.ResumeId && !e.IsDeleted,
                tracked: false,
                cancellationToken: cancellationToken,
                include: q => q.Include(e => e.Experiences)
                                .Include(e => e.Educations)
                                .Include(e => e.Certificates));

            if (resume is null)
            {
                return new ApiResponse<ResumeResponse>
                {
                    Success = false,
                    Message = "Resume not found"
                };
            }
            return new ApiResponse<ResumeResponse>
            {
                Success = true,
                Data = GetAllResumesByUserIdQueryHandler.MapToResponse(resume),
                Message = "Resume retrieved successfully"
            };
        }
    }
}
