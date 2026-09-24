using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.Features.Categories.Commands.CreateCategory;
using TalentHub.Application.Features.Categories.Commands.DeleteCategory;
using TalentHub.Application.Features.Categories.Commands.PermanentDeleteCategory;
using TalentHub.Application.Features.Categories.Commands.UpdateCategory;
using TalentHub.Application.Features.Categories.Queries.GetAllCategories;
using TalentHub.Application.Features.Categories.Queries.GetCategoryById;
using TalentHub.Infrastructure.Extensions;
using TalentHub.Infrastructure.Utilities;

namespace TalentHub.API.Controllers
{
    /// <summary>
    /// Provides endpoints for managing system categories.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Retrieves all categories in the system.
        /// </summary>
        /// <returns>A list of all categories.</returns>
        /// <response code="200">Categories retrieved successfully.</response>
        /// <response code="400">An error occurred while retrieving categories.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllCategoriesQuery());

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific category by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the category.</param>
        /// <returns>The requested category.</returns>
        /// <response code="200">Category retrieved successfully.</response>
        /// <response code="400">The category was not found or an error occurred.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetCategoryByIdQuery(id));

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Creates a new category.
        /// </summary>
        /// <param name="request">The category data to create.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The newly created category.</returns>
        /// <response code="200">Category created successfully.</response>
        /// <response code="400">Invalid category data or the category could not be created.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The user does not have permission to create categories.</response>
        [Authorize(Roles = $"{SystemRoles.SUPER_ADMIN},{SystemRoles.ADMIN}")]
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(
            [FromForm] CreateCategoryRequest request,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();

            if (userId is null)
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(
                new CreateCategoryCommand(userId, request),
                cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        /// <param name="id">The unique identifier of the category to update.</param>
        /// <param name="request">The updated category data.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The updated category.</returns>
        /// <response code="200">Category updated successfully.</response>
        /// <response code="400">Invalid category data or the category could not be updated.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The user does not have permission to update categories.</response>
        [Authorize(Roles = $"{SystemRoles.SUPER_ADMIN},{SystemRoles.ADMIN}")]
        [HttpPut("update/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(
            int id,
            [FromForm] UpdateCategoryRequest request,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();

            if (userId is null)
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(
                new UpdateCategoryCommand(userId, id, request),
                cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Soft deletes a category.
        /// </summary>
        /// <param name="id">The unique identifier of the category to delete.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The result of the delete operation.</returns>
        /// <response code="200">Category deleted successfully.</response>
        /// <response code="400">The category could not be deleted.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The user does not have permission to delete categories.</response>
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
                new DeleteCategoryCommand(id),
                cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Permanently deletes a category from the system.
        /// </summary>
        /// <param name="id">The unique identifier of the category to permanently delete.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The result of the permanent delete operation.</returns>
        /// <response code="200">Category permanently deleted successfully.</response>
        /// <response code="400">The category could not be permanently deleted.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The user does not have permission to permanently delete categories.</response>
        [Authorize(Roles = $"{SystemRoles.SUPER_ADMIN}")]
        [HttpDelete("permanent/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeletePermanent(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new PermanentDeleteCategoryCommand(id),
                cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}