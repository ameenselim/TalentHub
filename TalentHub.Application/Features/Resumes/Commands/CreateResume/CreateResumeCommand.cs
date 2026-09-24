using MediatR;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Features.Resumes.Queries.GetAllResumesByUserId;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.Resumes.Commands.CreateResume
{
    public record CreateResumeCommand(string UserId, CreateResumeRequest Request) : IRequest<ApiResponse<ResumeResponse>>;

    public class CreateResumeCommandHandler : IRequestHandler<CreateResumeCommand, ApiResponse<ResumeResponse>>
    {
        private readonly IRepository<Resume> _resumeRepository;
        private const int MaxResumesPerUser = 3;

        public CreateResumeCommandHandler(IRepository<Resume> resumeRepository)
        {
            _resumeRepository = resumeRepository;
        }

        public async Task<ApiResponse<ResumeResponse>> Handle(CreateResumeCommand command, CancellationToken cancellationToken)
        {
            var userId = command.UserId;
            var request = command.Request;

            var currentCount = await _resumeRepository.CountAsync(e => e.UserId == userId && !e.IsDeleted, cancellationToken);
            if (currentCount >= MaxResumesPerUser)
            {
                return new ApiResponse<ResumeResponse>
                {
                    Success = false,
                    Message = $"Maximum of {MaxResumesPerUser} resumes per user reached."
                };
            }
            var resume = new Resume
            {
                UserId = userId,
                Summary = request.Summary?.Trim(),
                ExpectedSalary = request.ExpectedSalary,
                PortfolioUrl = request.PortfolioUrl?.Trim(),
                GithubUrl = request.GithubUrl?.Trim(),
                LinkedinUrl = request.LinkedinUrl?.Trim(),
                YearsOfExperience = request.YearsOfExperience,
                CreatedBy = userId,
            };

            try
            {
                await _resumeRepository.CreateAsync(resume, cancellationToken);
                await _resumeRepository.CommitAsync(cancellationToken);
            }
            catch
            {
                return new ApiResponse<ResumeResponse>
                {
                    Success = false,
                    Message = "An error occurred while creating the resume."
                };
            }

            return new ApiResponse<ResumeResponse>
            {
                Success = true,
                Data = GetAllResumesByUserIdQueryHandler.MapToResponse(resume),
                Message = "Resume created successfully"
            };
        }
    }
}
