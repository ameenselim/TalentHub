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
    public class SkillsController : ControllerBase
    {
        private readonly ISkillServices _skillServices;

        public SkillsController(ISkillServices skillServices) 
        {
            _skillServices = skillServices;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _skillServices.GetAllSkillsAsync();
            if(!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _skillServices.GetSkillByIdAsync(id);
            if(!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [Authorize(Roles = $"{SystemRoles.SUPER_ADMIN},{SystemRoles.ADMIN}")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CreateSkillRequest request, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if (userId is null)
            {
                return Unauthorized();
            }
            var result = await _skillServices.CreateAsync(userId, request, cancellationToken);
            if(!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [Authorize(Roles = $"{SystemRoles.SUPER_ADMIN},{SystemRoles.ADMIN}")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateSkillRequest request, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if (userId is null)
            {
                return Unauthorized();
            }
            var result = await _skillServices.UpdateAsync(userId, id, request, cancellationToken);
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
            var result = await _skillServices.DeleteAsync(id, cancellationToken);
            if(!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [Authorize(Roles = $"{SystemRoles.SUPER_ADMIN}")]
        [HttpDelete("permanent/{id}")]
        public async Task<IActionResult> PermanentDelete(int id, CancellationToken cancellationToken)
        {
            var result = await _skillServices.PermanentDeleteAsync(id, cancellationToken);
            if(!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
