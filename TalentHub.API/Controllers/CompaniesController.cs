using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TalentHub.Infrastructure.Extensions;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.Interfaces.Services;

namespace TalentHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyServices _companyServices;

        public CompaniesController(ICompanyServices companyServices)
        {
            _companyServices = companyServices;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] CompanyFilterRequest companyRequest ,CancellationToken cancellationToken)
        {
            var result = await _companyServices.GetAllCompaniesAsync(companyRequest, cancellationToken:cancellationToken);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _companyServices.GetCompanyByIdAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [Authorize]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateCompanyRequest request,CancellationToken cancellationToken = default)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var result = await _companyServices.UpdateAsync(userId, id, request, cancellationToken);
            if (!result.Success)
            {
                if (result.Message == "Company not found")
                    return NotFound(result);

                if (result.Message == "You are not authorized to update this company.")
                {
                    return Forbid();
                }

                return BadRequest(result);
            }
            return Ok(result);
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id,CancellationToken cancellationToken = default)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var result = await _companyServices.DeleteAsync(userId, id, cancellationToken);
            if (!result.Success)
            {
                if (result.Message == "Company not found")
                    return NotFound(result);

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
