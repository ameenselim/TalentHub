using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.Features.Jobs.Commands.CreateJob;
using TalentHub.Application.Features.Jobs.Commands.DeleteJob;
using TalentHub.Application.Features.Jobs.Commands.UpdateJob;
using TalentHub.Application.Features.Jobs.Queries.GetAllJobs;
using TalentHub.Application.Features.Jobs.Queries.GetJobById;
using TalentHub.Infrastructure.Extensions;

namespace TalentHub.API.Controllers
{
    /// <summary>
    /// Provides endpoints for managing and retrieving job postings.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public JobsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Retrieves a specific job posting by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the job.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The requested job posting.</returns>
        /// <response code="200">Job retrieved successfully.</response>
        /// <response code="404">The specified job was not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetJobByIdQuery(id),
                cancellationToken);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Retrieves all job postings based on the specified filter criteria.
        /// </summary>
        /// <param name="jobFilter">
        /// Filtering and pagination parameters used to retrieve jobs.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>A filtered and paginated list of jobs.</returns>
        /// <response code="200">Jobs retrieved successfully.</response>
        /// <response code="400">The request is invalid or jobs could not be retrieved.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll(
            [FromQuery] JobFilterRequest jobFilter,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetAllJobsQuery(jobFilter),
                cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Creates a new job posting for a company.
        /// </summary>
        /// <param name="request">The job posting information.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The newly created job posting.</returns>
        /// <response code="200">Job created successfully.</response>
        /// <response code="400">The job data is invalid or the operation failed.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The user is not authorized to create jobs for the company.</response>
        /// <response code="404">The specified company or category was not found.</response>
        [Authorize]
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create(
            [FromBody] CreateJobRequest request,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(
                new CreateJobCommand(userId, request),
                cancellationToken);

            if (!result.Success)
            {
                if (result.Message == "You are not authorized to create jobs this company.")
                {
                    return Forbid();
                }

                if (result.Message == "category not found" ||
                    result.Message == "Company not found")
                {
                    return NotFound(result);
                }

                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Updates an existing job posting.
        /// </summary>
        /// <param name="id">The unique identifier of the job to update.</param>
        /// <param name="request">The updated job posting information.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The updated job posting.</returns>
        /// <response code="200">Job updated successfully.</response>
        /// <response code="400">The job update request is invalid or the operation failed.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The user is not authorized to update this job.</response>
        /// <response code="404">The specified job was not found.</response>
        [Authorize]
        [HttpPut("update/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateJobRequest request,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(
                new UpdateJobCommand(userId, id, request),
                cancellationToken);

            if (!result.Success)
            {
                if (result.Message == "You are not authorized to update this job.")
                {
                    return Forbid();
                }

                if (result.Message == "Job not found")
                {
                    return NotFound(result);
                }

                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Deletes an existing job posting.
        /// </summary>
        /// <param name="id">The unique identifier of the job to delete.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The result of the delete operation.</returns>
        /// <response code="200">Job deleted successfully.</response>
        /// <response code="400">The job could not be deleted.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The user is not authorized to delete this job.</response>
        /// <response code="404">The specified job was not found.</response>
        [Authorize]
        [HttpDelete("delete/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(
                new DeleteJobCommand(userId, id),
                cancellationToken);

            if (!result.Success)
            {
                if (result.Message == "You are not authorized to delete this job.")
                {
                    return Forbid();
                }

                if (result.Message == "Job not found")
                {
                    return NotFound(result);
                }

                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}