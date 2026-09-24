using MediatR;
using Microsoft.Extensions.Logging;
using System;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Application.Interfaces.Services;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.Resumes.Commands.DeleteResumeFile
{
    public record DeleteResumeFileCommand(string UserId, int ResumeId) : IRequest<ApiResponse<bool>>;

    public class DeleteResumeFileCommandHandler : IRequestHandler<DeleteResumeFileCommand, ApiResponse<bool>>
    {
        private readonly IRepository<Resume> _resumeRepository;
        private readonly ICloudinaryServices _cloudinaryServices;
        private readonly ILogger<DeleteResumeFileCommandHandler> _logger;

        public DeleteResumeFileCommandHandler(
            IRepository<Resume> resumeRepository,
            ICloudinaryServices cloudinaryServices,
            ILogger<DeleteResumeFileCommandHandler> logger)
        {
            _resumeRepository = resumeRepository;
            _cloudinaryServices = cloudinaryServices;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteResumeFileCommand command, CancellationToken cancellationToken)
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
            if (string.IsNullOrWhiteSpace(resume.ResumePublicId))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "No resume file to delete"
                };
            }
            var publicId = resume.ResumePublicId;
            try
            {
                resume.ResumeFile = null;
                resume.ResumePublicId = null;
                resume.UpdatedAt = DateTime.UtcNow;
                resume.UpdatedBy = userId;
                _resumeRepository.Update(resume);
                await _resumeRepository.CommitAsync(cancellationToken);
            }
            catch
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while removing the resume file."
                };
            }
            try
            {
                await _cloudinaryServices.DeleteAsync(publicId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete resume file from Cloudinary. PublicId: {PublicId}", publicId);
            }
            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Resume file deleted successfully"
            };
        }
    }
}
