using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.Interfaces.Services;
using TalentHub.Infrastructure.Identity;
using TalentHub.Infrastructure.Services;

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
        [HttpPost("register/company")]
        public async Task<IActionResult> RegisterAsCompanyAsync(RegisterCompanyRequest request)
        {
            var result = await _authServices.RegisterAsCompanyAsync(request);
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

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(Application.DTOs.Request.LoginRequest request)
        {
            var result = await _authServices.LoginAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
        {
            var result =await _authServices.RefreshTokenAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest request)
        {
            var result = await _authServices.ForgetPasswordAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPost("valid-otp")]
        public async Task<IActionResult> ValidOTP(ValidOTPRequest request)
        {
            var result = await _authServices.ValidOTPAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var result = await _authServices.ResetPasswordAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(RefreshTokenRequest request)
        {
            var response = await _authServices.LogoutAsync(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

    }
}
