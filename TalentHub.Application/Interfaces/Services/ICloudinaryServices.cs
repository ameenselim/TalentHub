using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace TalentHub.Application.Interfaces.Services
{
    public interface ICloudinaryServices
    {
        Task<(string Url, string PublicId)> UploadImageAsync(IFormFile file ,string folder, CancellationToken cancellationToken = default);
        Task DeleteAsync(string publicId, CancellationToken cancellationToken = default);
    }
}
