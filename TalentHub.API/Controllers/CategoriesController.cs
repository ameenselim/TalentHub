using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.Interfaces.Services;
using TalentHub.Infrastructure.Extensions;
using TalentHub.Infrastructure.Utilities;

namespace TalentHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryServices _categoryServices;

        public CategoriesController(ICategoryServices categoryServices) 
        {
            _categoryServices = categoryServices;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryServices.GetAllCategoriesAsync();
            if(!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _categoryServices.GetCategoryByIdAsync(id);
            if(!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [Authorize(Roles = $"{SystemRoles.SUPER_ADMIN},{SystemRoles.ADMIN}")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CreateCategoryRequest request, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if (userId is null)
            {
                return Unauthorized();
            }
            var result = await _categoryServices.CreateAsync(userId, request, cancellationToken);
            if(!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [Authorize(Roles = $"{SystemRoles.SUPER_ADMIN},{SystemRoles.ADMIN}")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateCategoryRequest request, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if (userId is null)
            {
                return Unauthorized();
            }
            var result = await _categoryServices.UpdateAsync(userId, id, request, cancellationToken);
            if(!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [Authorize(Roles = $"{SystemRoles.SUPER_ADMIN},{SystemRoles.ADMIN}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await _categoryServices.DeleteAsync(id, cancellationToken);
            if(!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [Authorize(Roles = $"{SystemRoles.SUPER_ADMIN}")]
        [HttpDelete("permanent/{id}")]
        public async Task<IActionResult> DeletePermanent(int id, CancellationToken cancellationToken)
        {
            var result = await _categoryServices.PermanentDeleteAsync(id, cancellationToken);
            if(!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
