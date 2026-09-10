using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.Interfaces.Services;
using TalentHub.Infrastructure.Extensions;

namespace TalentHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IJobServices _jobServices;

        public JobsController(IJobServices jobServices)
        {
            _jobServices = jobServices;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id,CancellationToken cancellationToken)
        {
            var result = await _jobServices.GetJobByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] JobFilterRequest jobFilter,CancellationToken cancellationToken)
        {
            var result = await _jobServices.GetAllJobsAsync(jobFilter,cancellationToken);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateJobRequest request,CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var result = await _jobServices.CreateJobAsync(userId, request, cancellationToken);
            if (!result.Success)
            {
                if (result.Message == "You are not authorized to create jobs this company.")
                {
                    return Forbid();
                }

                if (result.Message == "category not found"|| result.Message == "Company not found")
                {
                    return NotFound(result);
                }

                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int jobId, [FromBody] UpdateJobRequest request, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if(string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var result = await _jobServices.UpdateAsync(userId, jobId, request, cancellationToken);
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

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int jobId, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if(string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var result = await _jobServices.DeleteAsync(userId, jobId, cancellationToken);
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