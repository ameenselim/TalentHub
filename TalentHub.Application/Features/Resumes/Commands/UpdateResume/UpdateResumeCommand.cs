using MediatR;
using System;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Features.Resumes.Queries.GetAllResumesByUserId;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.Resumes.Commands.UpdateResume
{
    public record UpdateResumeCommand(string UserId, int ResumeId, UpdateResumeRequest Request) : IRequest<ApiResponse<ResumeResponse>>;

    public class UpdateResumeCommandHandler : IRequestHandler<UpdateResumeCommand, ApiResponse<ResumeResponse>>
    {
        private readonly IRepository<Resume> _resumeRepository;

        public UpdateResumeCommandHandler(IRepository<Resume> resumeRepository)
        {
            _resumeRepository = resumeRepository;
        }

        public async Task<ApiResponse<ResumeResponse>> Handle(UpdateResumeCommand command, CancellationToken cancellationToken)
        {
            var userId = command.UserId;
            var resumeId = command.ResumeId;
            var request = command.Request;

            var resume = await _resumeRepository.GetOneAsync(
                e => e.Id == resumeId && e.UserId == userId && !e.IsDeleted,
                cancellationToken: cancellationToken);

            if (resume is null)
            {
                return new ApiResponse<ResumeResponse>
                {
                    Success = false,
                    Message = "Resume not found"
                };
            }
            try
            {
                resume.Summary = request.Summary?.Trim();
                resume.ExpectedSalary = request.ExpectedSalary;
                resume.PortfolioUrl = request.PortfolioUrl?.Trim();
                resume.GithubUrl = request.GithubUrl?.Trim();
                resume.LinkedinUrl = request.LinkedinUrl?.Trim();
                resume.YearsOfExperience = request.YearsOfExperience;
                resume.UpdatedAt = DateTime.UtcNow;
                resume.UpdatedBy = userId;

                _resumeRepository.Update(resume);
                await _resumeRepository.CommitAsync(cancellationToken);
            }
            catch
            {
                return new ApiResponse<ResumeResponse>
                {
                    Success = false,
                    Message = "An error occurred while updating the resume."
                };
            }

            return new ApiResponse<ResumeResponse>
            {
                Success = true,
                Data = GetAllResumesByUserIdQueryHandler.MapToResponse(resume),
                Message = "Resume updated successfully"
            };
        }
    }
}
