using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.Features.CompanyImages.Commands.AddCompanyImage;
using TalentHub.Application.Features.CompanyImages.Commands.DeleteCompanyImage;
using TalentHub.Application.Features.CompanyImages.Commands.SetAsCover;
using TalentHub.Application.Features.CompanyImages.Commands.UpdateCompanyImage;
using TalentHub.Application.Features.CompanyImages.Queries.GetCompanyImageById;
using TalentHub.Application.Features.CompanyImages.Queries.GetCompanyImages;
using TalentHub.Infrastructure.Extensions;

namespace TalentHub.API.Controllers
{
    /// <summary>
    /// Provides endpoints for managing company images.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyImagesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CompanyImagesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Retrieves all images associated with a specific company.
        /// </summary>
        /// <param name="companyId">The unique identifier of the company.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>A list of images belonging to the specified company.</returns>
        /// <response code="200">Company images retrieved successfully.</response>
        /// <response code="404">The company or its images were not found.</response>
        [HttpGet("companies/{companyId}/images")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCompanyImages(
            int companyId,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(
                new GetCompanyImagesQuery(companyId),
                cancellationToken);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific image belonging to a company.
        /// </summary>
        /// <param name="companyId">The unique identifier of the company.</param>
        /// <param name="imageId">The unique identifier of the image.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The requested company image.</returns>
        /// <response code="200">Company image retrieved successfully.</response>
        /// <response code="404">The company or image was not found.</response>
        [HttpGet("companies/{companyId}/images/{imageId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCompanyImageById(
            int companyId,
            int imageId,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(
                new GetCompanyImageByIdQuery(imageId, companyId),
                cancellationToken);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Adds a new image to a company.
        /// </summary>
        /// <param name="companyId">The unique identifier of the company.</param>
        /// <param name="request">The company image information and uploaded file.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The newly added company image.</returns>
        /// <response code="200">Company image added successfully.</response>
        /// <response code="400">The image data is invalid or the operation failed.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The user is not authorized to add images for this company.</response>
        /// <response code="404">The specified company was not found.</response>
        [Authorize]
        [HttpPost("companies/{companyId}/images")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateCompanyImage(
            int companyId,
            [FromForm] AddCompanyImageRequest request,
            CancellationToken cancellationToken = default)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(
                new AddCompanyImageCommand(userId, companyId, request),
                cancellationToken);

            if (!result.Success)
            {
                if (result.Message == "Company not found")
                {
                    return NotFound(result);
                }

                if (result.Message == "You are not authorized to add images for this company.")
                {
                    return Forbid();
                }

                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Updates the information of an existing company image.
        /// </summary>
        /// <param name="companyId">The unique identifier of the company.</param>
        /// <param name="imageId">The unique identifier of the image.</param>
        /// <param name="request">The updated image information.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The updated company image.</returns>
        /// <response code="200">Company image updated successfully.</response>
        /// <response code="400">The image update request is invalid or the operation failed.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The user is not authorized to update images for this company.</response>
        /// <response code="404">The specified company or image was not found.</response>
        [HttpPatch("companies/{companyId}/images/{imageId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCompanyImage(
            int imageId,
            int companyId,
            [FromBody] UpdateCompanyImageRequest request,
            CancellationToken cancellationToken = default)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(
                new UpdateCompanyImageCommand(
                    userId,
                    imageId,
                    companyId,
                    request),
                cancellationToken);

            if (!result.Success)
            {
                if (result.Message == "Company not found")
                {
                    return NotFound(result);
                }

                if (result.Message == "You are not authorized to update images for this company.")
                {
                    return Forbid();
                }

                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Sets a company image as the cover image.
        /// </summary>
        /// <param name="companyId">The unique identifier of the company.</param>
        /// <param name="imageId">The unique identifier of the image to set as cover.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The result of the set-as-cover operation.</returns>
        /// <response code="200">The image was successfully set as the company cover.</response>
        /// <response code="400">The operation failed.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The user is not authorized to set cover images for this company.</response>
        /// <response code="404">The specified company or image was not found.</response>
        [HttpPatch("companies/{companyId}/images/{imageId}/set-as-cover")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetAsCover(
            int companyId,
            int imageId,
            CancellationToken cancellationToken = default)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(
                new SetAsCoverCommand(userId, companyId, imageId),
                cancellationToken);

            if (!result.Success)
            {
                if (result.Message == "Company not found")
                {
                    return NotFound(result);
                }

                if (result.Message == "You are not authorized to set cover images for this company.")
                {
                    return Forbid();
                }

                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Deletes an image from a company.
        /// </summary>
        /// <param name="companyId">The unique identifier of the company.</param>
        /// <param name="imageId">The unique identifier of the image to delete.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The result of the delete operation.</returns>
        /// <response code="200">Company image deleted successfully.</response>
        /// <response code="400">The image could not be deleted.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The user is not authorized to delete images for this company.</response>
        /// <response code="404">The specified company or image was not found.</response>
        [HttpDelete("companies/{companyId}/images/{imageId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCompanyImage(
            int imageId,
            int companyId,
            CancellationToken cancellationToken = default)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(
                new DeleteCompanyImageCommand(userId, imageId, companyId),
                cancellationToken);

            if (!result.Success)
            {
                if (result.Message == "Company not found")
                {
                    return NotFound(result);
                }

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