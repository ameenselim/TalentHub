using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.Features.Companies.Commands.DeleteCompany;
using TalentHub.Application.Features.Companies.Commands.UpdateCompany;
using TalentHub.Application.Features.Companies.Queries.GetAllCompanies;
using TalentHub.Application.Features.Companies.Queries.GetCompanyById;
using TalentHub.Infrastructure.Extensions;

namespace TalentHub.API.Controllers
{
    /// <summary>
    /// Provides endpoints for managing and retrieving companies.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CompaniesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Retrieves all companies based on the specified filter criteria.
        /// </summary>
        /// <param name="companyRequest">
        /// The filter and pagination parameters used to retrieve companies.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the request.
        /// </param>
        /// <returns>A paginated list of companies.</returns>
        /// <response code="200">Companies retrieved successfully.</response>
        /// <response code="400">An error occurred while retrieving companies.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll(
            [FromQuery] CompanyFilterRequest companyRequest,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetAllCompaniesQuery(companyRequest),
                cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific company by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the company.</param>
        /// <param name="cancellationToken">
        /// Token used to cancel the request.
        /// </param>
        /// <returns>The requested company.</returns>
        /// <response code="200">Company retrieved successfully.</response>
        /// <response code="400">The request failed or the company could not be retrieved.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetCompanyByIdQuery(id),
                cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Updates an existing company.
        /// </summary>
        /// <param name="id">The unique identifier of the company to update.</param>
        /// <param name="request">The updated company information.</param>
        /// <param name="cancellationToken">
        /// Token used to cancel the request.
        /// </param>
        /// <returns>The updated company.</returns>
        /// <response code="200">Company updated successfully.</response>
        /// <response code="400">The company update request is invalid or the operation failed.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The authenticated user is not authorized to update this company.</response>
        /// <response code="404">The specified company was not found.</response>
        [Authorize]
        [HttpPut("update/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            int id,
            [FromForm] UpdateCompanyRequest request,
            CancellationToken cancellationToken = default)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(
                new UpdateCompanyCommand(userId, id, request),
                cancellationToken);

            if (!result.Success)
            {
                if (result.Message == "Company not found")
                {
                    return NotFound(result);
                }

                if (result.Message == "You are not authorized to update this company.")
                {
                    return Forbid();
                }

                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Deletes a company.
        /// </summary>
        /// <param name="id">The unique identifier of the company to delete.</param>
        /// <param name="cancellationToken">
        /// Token used to cancel the request.
        /// </param>
        /// <returns>The result of the delete operation.</returns>
        /// <response code="200">Company deleted successfully.</response>
        /// <response code="400">The company could not be deleted.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The authenticated user is not authorized to delete this company.</response>
        /// <response code="404">The specified company was not found.</response>
        [Authorize]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken = default)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(
                new DeleteCompanyCommand(userId, id),
                cancellationToken);

            if (!result.Success)
            {
                if (result.Message == "Company not found")
                {
                    return NotFound(result);
                }

                if (result.Message == "You are not authorized to delete this company.")
                {
                    return Forbid();
                }

                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}