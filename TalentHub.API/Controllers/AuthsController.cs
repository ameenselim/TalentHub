using MediatR;
using Microsoft.AspNetCore.Mvc;
using TalentHub.Application.DTOs.Request;
using TalentHub.Application.Features.Auth.Commands.ConfirmEmail;
using TalentHub.Application.Features.Auth.Commands.ForgetPassword;
using TalentHub.Application.Features.Auth.Commands.Login;
using TalentHub.Application.Features.Auth.Commands.Logout;
using TalentHub.Application.Features.Auth.Commands.RefreshToken;
using TalentHub.Application.Features.Auth.Commands.RegisterCompany;
using TalentHub.Application.Features.Auth.Commands.RegisterJobSeeker;
using TalentHub.Application.Features.Auth.Commands.ResendEmailConfirmation;
using TalentHub.Application.Features.Auth.Commands.ResetPassword;
using TalentHub.Application.Features.Auth.Commands.ValidOTP;

namespace TalentHub.API.Controllers
{
    /// <summary>
    /// Provides authentication and account management endpoints.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Registers a new job seeker account.
        /// </summary>
        /// <param name="request">The job seeker registration information.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The result of the registration operation.</returns>
        /// <response code="200">Job seeker registered successfully.</response>
        /// <response code="400">Registration failed due to invalid or duplicate data.</response>
        [HttpPost("register/job-seeker")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterAsJobSeeker(
            RegisterUserRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new RegisterJobSeekerCommand(request),
                cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Registers a new company account.
        /// </summary>
        /// <param name="request">The company registration information.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The result of the company registration operation.</returns>
        /// <response code="200">Company registered successfully.</response>
        /// <response code="400">Registration failed due to invalid or duplicate data.</response>
        [HttpPost("register/company")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterAsCompany(
            RegisterCompanyRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new RegisterCompanyCommand(request),
                cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Confirms a user's email address using the confirmation token.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="token">The email confirmation token.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The result of the email confirmation operation.</returns>
        /// <response code="200">Email confirmed successfully.</response>
        /// <response code="400">The confirmation token is invalid, expired, or the operation failed.</response>
        [HttpGet("confirm-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmEmail(
            [FromQuery] string userId,
            [FromQuery] string token,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new ConfirmEmailCommand(userId, token),
                cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Resends the email confirmation message to a user.
        /// </summary>
        /// <param name="request">The user's email confirmation request.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The result of the resend operation.</returns>
        /// <response code="200">Confirmation email sent successfully.</response>
        /// <response code="400">The request is invalid or the operation failed.</response>
        [HttpPost("resend-email-confirmation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResendEmailConfirmation(
            ResendEmailConfirmationRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new ResendEmailConfirmationCommand(request),
                cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Authenticates a user and generates authentication tokens.
        /// </summary>
        /// <param name="request">The user's login credentials.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The authentication result containing the generated tokens.</returns>
        /// <response code="200">Login successful.</response>
        /// <response code="400">Invalid credentials or login failed.</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login(
            LoginRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new LoginCommand(request),
                cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Generates a new access token using a valid refresh token.
        /// </summary>
        /// <param name="request">The refresh token information.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The refreshed authentication tokens.</returns>
        /// <response code="200">Access token refreshed successfully.</response>
        /// <response code="400">The refresh token is invalid, expired, or revoked.</response>
        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshToken(
            RefreshTokenRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new RefreshTokenCommand(request),
                cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Initiates the password recovery process by sending an OTP.
        /// </summary>
        /// <param name="request">The user's password recovery information.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The result of the password recovery request.</returns>
        /// <response code="200">Password recovery request processed successfully.</response>
        /// <response code="400">The request is invalid or the operation failed.</response>
        [HttpPost("forget-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgetPassword(
            ForgetPasswordRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new ForgetPasswordCommand(request),
                cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Validates the OTP sent during the password recovery process.
        /// </summary>
        /// <param name="request">The OTP validation information.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The result of the OTP validation.</returns>
        /// <response code="200">OTP validated successfully.</response>
        /// <response code="400">The OTP is invalid, expired, or already used.</response>
        [HttpPost("valid-otp")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ValidOTP(
            ValidOTPRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new ValidOTPCommand(request),
                cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Resets the user's password after successful OTP validation.
        /// </summary>
        /// <param name="request">The new password and password reset information.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The result of the password reset operation.</returns>
        /// <response code="200">Password reset successfully.</response>
        /// <response code="400">The request is invalid or the password could not be reset.</response>
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new ResetPasswordCommand(request),
                cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Logs out the user by invalidating the provided refresh token.
        /// </summary>
        /// <param name="request">The refresh token to invalidate.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The result of the logout operation.</returns>
        /// <response code="200">Logout completed successfully.</response>
        /// <response code="400">The refresh token is invalid or the logout operation failed.</response>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout(
            RefreshTokenRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new LogoutCommand(request),
                cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}