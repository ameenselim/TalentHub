using MediatR;
using Microsoft.Extensions.Logging;
using System;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Application.Interfaces.Services;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.Resumes.Commands.DeleteResume
{
    public record DeleteResumeCommand(string UserId, int ResumeId) : IRequest<ApiResponse<bool>>;

    public class DeleteResumeCommandHandler : IRequestHandler<DeleteResumeCommand, ApiResponse<bool>>
    {
        private readonly IRepository<Resume> _resumeRepository;
        private readonly ICloudinaryServices _cloudinaryServices;
        private readonly ILogger<DeleteResumeCommandHandler> _logger;

        public DeleteResumeCommandHandler(
            IRepository<Resume> resumeRepository,
            ICloudinaryServices cloudinaryServices,
            ILogger<DeleteResumeCommandHandler> logger)
        {
            _resumeRepository = resumeRepository;
            _cloudinaryServices = cloudinaryServices;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteResumeCommand command, CancellationToken cancellationToken)
        {
            var userId = command.UserId;
            var resumeId = command.ResumeId;

            var resume = await _resumeRepository.GetOneAsync(
                e => e.Id == resumeId && e.UserId == userId && !e.IsDeleted,
                cancellationToken: cancellationToken);

            if (resume is null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Resume not found"
                };
            }
            var oldPublicId = resume.ResumePublicId;
            try
            {
                resume.IsDeleted = true;
                resume.DeletedAt = DateTime.UtcNow;
                _resumeRepository.Update(resume);
                await _resumeRepository.CommitAsync(cancellationToken);
            }
            catch
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while deleting the resume."
                };
            }

            if (!string.IsNullOrWhiteSpace(oldPublicId))
            {
                try
                {
                    await _cloudinaryServices.DeleteAsync(oldPublicId, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete resume file from Cloudinary. PublicId: {PublicId}", oldPublicId);
                }
            }
            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Resume deleted successfully",
            };
        }
    }
}
