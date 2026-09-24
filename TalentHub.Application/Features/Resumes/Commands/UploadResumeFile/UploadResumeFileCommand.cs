using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using TalentHub.Application.DTOs.Response;
using TalentHub.Application.Interfaces.Repository;
using TalentHub.Application.Interfaces.Services;
using TalentHub.Domain.Entities;

namespace TalentHub.Application.Features.Resumes.Commands.UploadResumeFile
{
    public record UploadResumeFileCommand(string UserId, int ResumeId, IFormFile File) : IRequest<ApiResponse<string>>;

    public class UploadResumeFileCommandHandler : IRequestHandler<UploadResumeFileCommand, ApiResponse<string>>
    {
        private readonly IRepository<Resume> _resumeRepository;
        private readonly ICloudinaryServices _cloudinaryServices;
        private readonly ILogger<UploadResumeFileCommandHandler> _logger;

        private const string ResumeFolder = "TalentHub/Resumes";

        public UploadResumeFileCommandHandler(
            IRepository<Resume> resumeRepository,
            ICloudinaryServices cloudinaryServices,
            ILogger<UploadResumeFileCommandHandler> logger)
        {
            _resumeRepository = resumeRepository;
            _cloudinaryServices = cloudinaryServices;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(UploadResumeFileCommand command, CancellationToken cancellationToken)
        {
            var userId = command.UserId;
            var resumeId = command.ResumeId;
            var file = command.File;

            var resume = await _resumeRepository.GetOneAsync(
                e => e.Id == resumeId && e.UserId == userId && !e.IsDeleted,
                cancellationToken: cancellationToken);

            if (resume is null)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Resume not found"
                };
            }
            if (file is null || file.Length == 0)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "File is required."
                };
            }
            var oldPublicId = resume.ResumePublicId;
            string? newPublicId = null;
            try
            {
                var uploadResult = await _cloudinaryServices.UploadRawFileAsync(file, ResumeFolder, cancellationToken);

                resume.ResumeFile = uploadResult.Url;
                resume.ResumePublicId = uploadResult.PublicId;
                newPublicId = uploadResult.PublicId;
                resume.UpdatedAt = DateTime.UtcNow;
                resume.UpdatedBy = userId;

                _resumeRepository.Update(resume);
                await _resumeRepository.CommitAsync(cancellationToken);
            }
            catch
            {
                if (!string.IsNullOrWhiteSpace(newPublicId))
                {
                    try
                    {
                        await _cloudinaryServices.DeleteAsync(newPublicId, CancellationToken.None);
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogWarning(cleanupEx, "Failed to cleanup newly uploaded resume file {PublicId}", newPublicId);
                    }
                }
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while uploading the resume file."
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
                    _logger.LogWarning(ex, "Failed to delete old resume file {PublicId}", oldPublicId);
                }
            }
            return new ApiResponse<string>
            {
                Success = true,
                Data = resume.ResumeFile,
                Message = "Resume file uploaded successfully"
            };
        }
    }
}
