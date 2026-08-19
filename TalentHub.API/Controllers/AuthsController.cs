using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.Interfaces.Services;
using TalentHub.Infrastructure.Identity;

namespace TalentHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthsController : ControllerBase
    {
        private readonly IAuthServices _authServices;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthsController(IAuthServices authServices ,UserManager<ApplicationUser> userManager)
        {
            _authServices = authServices;
            _userManager = userManager;
        }
        [HttpPost("register/job-seeker")]
        public async Task<IActionResult> RegisterAsJobSeekerAsync(RegisterUserRequest request)
        {
            var result = await _authServices.RegisterAsJobSeekerAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId,[FromQuery] string token)
        {
            var result = await _authServices.ConfirmEmail(userId, token);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        [HttpPost("resend-email-confirmation")]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationRequest request)
        {
            var result = await _authServices.ResendEmailConfirmation(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);

        }
    }
}
