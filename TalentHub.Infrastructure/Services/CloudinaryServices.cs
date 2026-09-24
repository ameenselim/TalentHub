using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using TalentHub.Application.Common.Settings;
using TalentHub.Application.Interfaces.Services;

namespace TalentHub.Infrastructure.Services
{
    public class CloudinaryServices : ICloudinaryServices
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryServices> _logger;

        private static readonly Dictionary<string, string[]> AllowedFileTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                [".jpg"] = new[] { "image/jpeg", "image/jpg" },
                [".jpeg"] = new[] { "image/jpeg", "image/jpg" },
                [".png"] = new[] { "image/png" },
                [".webp"] = new[] { "image/webp" }
            };
        private static readonly string[] AllowedResumeExtensions = { ".pdf", ".doc", ".docx" };

        private const long MaxResumeFileSizeBytes = 10 * 1024 * 1024; // 10MB
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

        public CloudinaryServices(IOptions<CloudinarySettings> options, ILogger<CloudinaryServices> logger)
        {
            var settings = options.Value;
            var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
            _cloudinary = new Cloudinary(account);
            _logger = logger;
        }

        public async Task<(string Url, string PublicId)> UploadImageAsync(IFormFile file, string folder, CancellationToken cancellationToken = default)
        {
            // Validate the file before proceeding with the upload
            ValidateFile(file);

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            ImageUploadResult result;
            try
            {
                result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cloudinary upload threw an exception for file {FileName}", file.FileName);
                throw new CloudinaryUploadException("An error occurred while uploading the file.", ex);
            }

            if (result.Error != null)
            {
                _logger.LogError("Cloudinary upload failed for file {FileName}: {Error}", file.FileName, result.Error.Message);
                throw new CloudinaryUploadException(result.Error.Message);
            }

            _logger.LogInformation("File {FileName} uploaded successfully with PublicId {PublicId}", file.FileName, result.PublicId);

            return (result.SecureUrl.ToString(), result.PublicId);
        }

        public async Task DeleteAsync(string publicId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                return;

            var deleteParams = new DeletionParams(publicId);

            DeletionResult result;
            try
            {
                result = await _cloudinary.DestroyAsync(deleteParams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cloudinary deletion threw an exception for PublicId {PublicId}", publicId);
                throw new CloudinaryDeleteException("An error occurred while deleting the file.", ex);
            }

            if (result.Error != null)
            {
                _logger.LogError("Cloudinary deletion failed for PublicId {PublicId}: {Error}", publicId, result.Error.Message);
                throw new CloudinaryDeleteException(result.Error.Message);
            }

            _logger.LogInformation("File with PublicId {PublicId} deleted successfully", publicId);
        }
        public async Task<(string Url, string PublicId)> UploadRawFileAsync(IFormFile file, string folder, CancellationToken cancellationToken = default)
        {
            ValidateRawFile(file);

            await using var stream = file.OpenReadStream();

            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            RawUploadResult result;
            try
            {
                result = await _cloudinary.UploadAsync(uploadParams, cancellationToken:cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cloudinary raw upload threw an exception for file {FileName}", file.FileName);
                throw new CloudinaryUploadException("An error occurred while uploading the file.", ex);
            }

            if (result.Error != null)
            {
                _logger.LogError("Cloudinary raw upload failed for file {FileName}: {Error}", file.FileName, result.Error.Message);
                throw new CloudinaryUploadException(result.Error.Message);
            }

            return (result.SecureUrl.ToString(), result.PublicId);
        }

        private static void ValidateRawFile(IFormFile file)
        {
            if (file is null || file.Length == 0)
                throw new ArgumentException("Invalid file.");

            if (file.Length > MaxResumeFileSizeBytes)
                throw new ArgumentException(
                    $"File size exceeds the maximum allowed size of {MaxResumeFileSizeBytes / (1024 * 1024)}MB.");

            var extension = Path.GetExtension(file.FileName);

            if (string.IsNullOrWhiteSpace(extension) ||
                !AllowedResumeExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Invalid file type. Only PDF, DOC, and DOCX are allowed.");
            }
        }

        private static void ValidateFile(IFormFile file)
        {
            if (file is null || file.Length == 0)
                throw new ArgumentException("Invalid file.");

            if (file.Length > MaxFileSizeBytes)
                throw new ArgumentException(
                    $"File size exceeds the maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)}MB.");

            var extension = Path.GetExtension(file.FileName);

            if (string.IsNullOrWhiteSpace(extension) ||
                !AllowedFileTypes.TryGetValue(extension, out var allowedContentTypes))
            {
                throw new ArgumentException(
                    "Invalid file type. Only JPEG, PNG, and WEBP are allowed.");
            }

            // بعض الـ clients (زي Postman) بتبعت Content-Type عام "application/octet-stream"
            // لما مش متأكدة من نوع الملف، فبنعتبره حالة استثناء ونثق في الامتداد بدل ما نرفض الملف
            if (!string.Equals(file.ContentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase))
            {
                if (!allowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
                {
                    throw new ArgumentException($"Invalid file content type: {file.ContentType}");
                }
            }
        }
    }

    public class CloudinaryUploadException : Exception
    {
        public CloudinaryUploadException(string message) : base(message) { }
        public CloudinaryUploadException(string message, Exception inner) : base(message, inner) { }
    }

    public class CloudinaryDeleteException : Exception
    {
        public CloudinaryDeleteException(string message) : base(message) { }
        public CloudinaryDeleteException(string message, Exception inner) : base(message, inner) { }
    }
}