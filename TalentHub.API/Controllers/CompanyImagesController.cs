using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.Interfaces.Services;
using TalentHub.Infrastructure.Extensions;
using static System.Net.Mime.MediaTypeNames;

namespace TalentHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyImagesController : ControllerBase
    {
        private readonly ICompanyImageServices _companyImageServices;

        public CompanyImagesController(ICompanyImageServices companyImageServices)
        {
            _companyImageServices = companyImageServices;
        }
        [HttpGet("companies/{companyId}/images")]
        public async Task<IActionResult> GetCompanyImages(int companyId, CancellationToken cancellationToken = default)
        {
            var result = await _companyImageServices.GetCompanyImagesAsync(companyId, cancellationToken);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet("companies/{companyId}/images/{imageId}")]
        public async Task<IActionResult> GetCompanyImageById(int companyId, int imageId, CancellationToken cancellationToken = default)
        {
            var result = await _companyImageServices.GetCompanyImageByIdAsync(imageId, companyId, cancellationToken);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
        [Authorize]
        [HttpPost("companies/{companyId}/images")]
        public async Task<IActionResult> CreateCompanyImage(int companyId, [FromForm] AddCompanyImageRequest request, CancellationToken cancellationToken = default)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var result = await _companyImageServices.AddImageAsync(userId, companyId, request, cancellationToken: cancellationToken);
            if (!result.Success)
            {
                if (result.Message == "Company not found")
                    return NotFound(result);

                if (result.Message == "You are not authorized to add images for this company.")
                {
                    return Forbid();
                }

                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPatch("companies/{companyId}/images/{imageId}")]
        public async Task<IActionResult> UpdateCompanyImage(int imageId, int companyId, [FromBody] UpdateCompanyImageRequest request, CancellationToken cancellationToken = default)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var result = await _companyImageServices.UpdateImageAsync(userId, imageId, companyId, request, cancellationToken: cancellationToken);
            if (!result.Success)
            {
                if (result.Message == "Company not found")
                    return NotFound(result);

                if (result.Message == "You are not authorized to update images for this company.")
                {
                    return Forbid();
                }

                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPatch("companies/{companyId}/images/{imageId}/set-as-cover")]
        public async Task<IActionResult> SetAsCover(int companyId, int imageId, CancellationToken cancellationToken = default)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var result = await _companyImageServices.SetAsCoverAsync(userId, companyId, imageId, cancellationToken);
            if (!result.Success)
            {
                if (result.Message == "Company not found")
                    return NotFound(result);

                if (result.Message == "You are not authorized to set cover images for this company.")
                {
                    return Forbid();
                }

                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete("companies/{companyId}/images/{imageId}")]
        public async Task<IActionResult> DeleteCompanyImage(int imageId, int companyId, CancellationToken cancellationToken = default)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var result = await _companyImageServices.DeleteImageAsync(userId, imageId, companyId, cancellationToken: cancellationToken);
            if (!result.Success)
            {
                if (result.Message == "Company not found")
                    return NotFound(result);

                if (result.Message == "You are not authorized to delete images for this company.")
                {
                    return Forbid();
                }

                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
