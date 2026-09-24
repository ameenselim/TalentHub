using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.Features.Skills.Commands.CreateSkill;
using TalentHub.Application.Features.Skills.Commands.DeleteSkill;
using TalentHub.Application.Features.Skills.Commands.PermanentDeleteSkill;
using TalentHub.Application.Features.Skills.Commands.UpdateSkill;
using TalentHub.Application.Features.Skills.Queries.GetAllSkills;
using TalentHub.Application.Features.Skills.Queries.GetSkillById;
using TalentHub.Infrastructure.Extensions;
using TalentHub.Infrastructure.Utilities;

namespace TalentHub.API.Controllers
{
    /// <summary>
    /// Provides endpoints for managing skills.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SkillsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SkillsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Retrieves all skills in the system.
        /// </summary>
        /// <returns>A list of all skills.</returns>
        /// <response code="200">Skills retrieved successfully.</response>
        /// <response code="400">An error occurred while retrieving skills.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllSkillsQuery());

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific skill by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the skill.</param>
        /// <returns>The requested skill.</returns>
        /// <response code="200">Skill retrieved successfully.</response>
        /// <response code="400">The skill could not be retrieved.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetSkillByIdQuery(id));

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Creates a new skill.
        /// </summary>
        /// <param name="request">The skill information to create.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The newly created skill.</returns>
        /// <response code="200">Skill created successfully.</response>
        /// <response code="400">The skill data is invalid or the operation failed.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The user does not have permission to create skills.</response>
        [Authorize(Roles = $"{SystemRoles.SUPER_ADMIN},{SystemRoles.ADMIN}")]
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(
            [FromForm] CreateSkillRequest request,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();

            if (userId is null)
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(
                new CreateSkillCommand(userId, request),
                cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Updates an existing skill.
        /// </summary>
        /// <param name="id">The unique identifier of the skill to update.</param>
        /// <param name="request">The updated skill information.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The updated skill.</returns>
        /// <response code="200">Skill updated successfully.</response>
        /// <response code="400">The skill data is invalid or the operation failed.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The user does not have permission to update skills.</response>
        [Authorize(Roles = $"{SystemRoles.SUPER_ADMIN},{SystemRoles.ADMIN}")]
        [HttpPut("update/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(
            int id,
            [FromForm] UpdateSkillRequest request,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();

            if (userId is null)
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(
                new UpdateSkillCommand(userId, id, request),
                cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Soft deletes a skill.
        /// </summary>
        /// <param name="id">The unique identifier of the skill to delete.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The result of the delete operation.</returns>
        /// <response code="200">Skill deleted successfully.</response>
        /// <response code="400">The skill could not be deleted.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The user does not have permission to delete skills.</response>
        [Authorize(Roles = $"{SystemRoles.SUPER_ADMIN},{SystemRoles.ADMIN}")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new DeleteSkillCommand(id),
                cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Permanently deletes a skill from the system.
        /// </summary>
        /// <param name="id">The unique identifier of the skill to permanently delete.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The result of the permanent delete operation.</returns>
        /// <response code="200">Skill permanently deleted successfully.</response>
        /// <response code="400">The skill could not be permanently deleted.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">Only super administrators can permanently delete skills.</response>
        [Authorize(Roles = $"{SystemRoles.SUPER_ADMIN}")]
        [HttpDelete("permanent/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> PermanentDelete(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new PermanentDeleteSkillCommand(id),
                cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}