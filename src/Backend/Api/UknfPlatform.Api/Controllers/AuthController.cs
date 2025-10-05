using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UknfPlatform.Application.Auth.Authentication.Commands;
using UknfPlatform.Application.Auth.Authentication.DTOs;
using UknfPlatform.Application.Auth.Authentication.Queries;
using UknfPlatform.Domain.Shared.Exceptions;

namespace UknfPlatform.Api.Controllers;

/// <summary>
/// Authentication controller for user registration and login
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Register a new external user
    /// </summary>
    /// <param name="command">Registration data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Registration result with user ID and confirmation message</returns>
    /// <response code="201">User registered successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="409">Email or PESEL already exists</response>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterUserResponse>> RegisterAsync(
        [FromBody] RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            _logger.LogInformation("User registered successfully: {UserId}", result.UserId);
            return Created($"/api/auth/users/{result.UserId}", result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            _logger.LogWarning("Registration conflict: {Message}", ex.Message);
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return BadRequest(new { error = "Registration failed. Please try again." });
        }
    }

    /// <summary>
    /// Activate a user account using activation token from email
    /// </summary>
    /// <param name="token">Activation token from email link</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Activation result with user ID and message</returns>
    /// <response code="200">Account activated successfully</response>
    /// <response code="400">Invalid, expired, or already used token</response>
    [AllowAnonymous]
    [HttpGet("activate")]
    [ProducesResponseType(typeof(ActivateAccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ActivateAccountResponse>> ActivateAccountAsync(
        [FromQuery] string token,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new ActivateAccountCommand(token);
            var result = await _mediator.Send(command, cancellationToken);
            _logger.LogInformation("Account activated successfully for user: {UserId}", result.UserId);
            return Ok(result);
        }
        catch (InvalidTokenException ex)
        {
            _logger.LogWarning("Activation failed: Invalid token");
            return BadRequest(new { error = ex.Message });
        }
        catch (TokenExpiredException ex)
        {
            _logger.LogWarning("Activation failed: Token expired");
            return BadRequest(new { error = ex.Message });
        }
        catch (TokenAlreadyUsedException ex)
        {
            _logger.LogWarning("Activation failed: Token already used");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during account activation");
            return BadRequest(new { error = "Account activation failed. Please try again or request a new activation link." });
        }
    }

    /// <summary>
    /// Resend activation email to a user
    /// </summary>
    /// <param name="command">Resend request with email</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generic success message</returns>
    /// <response code="200">Request processed (doesn't reveal if email exists)</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="429">Too many requests - rate limit exceeded (3 requests per hour)</response>
    [AllowAnonymous]
    [HttpPost("resend-activation")]
    [EnableRateLimiting("resend-activation")]
    [ProducesResponseType(typeof(ResendActivationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ResendActivationResponse>> ResendActivationAsync(
        [FromBody] ResendActivationCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            _logger.LogInformation("Resend activation request processed");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during resend activation");
            return BadRequest(new { error = "Failed to process request. Please try again." });
        }
    }

    /// <summary>
    /// Set initial password for newly activated account
    /// </summary>
    /// <param name="command">Set password request with token and password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message with redirect URL to login</returns>
    /// <response code="200">Password set successfully</response>
    /// <response code="400">Invalid request data or policy violation</response>
    /// <response code="429">Too many password attempts</response>
    [AllowAnonymous]
    [HttpPost("set-password")]
    [EnableRateLimiting("set-password")]
    [ProducesResponseType(typeof(SetPasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SetPasswordResponse>> SetPasswordAsync(
        [FromBody] SetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            _logger.LogInformation("Password set successfully for user: {UserId}", result.UserId);
            return Ok(result);
        }
        catch (InvalidTokenException ex)
        {
            _logger.LogWarning("Set password failed: Invalid token");
            return BadRequest(new { error = ex.Message });
        }
        catch (TokenExpiredException ex)
        {
            _logger.LogWarning("Set password failed: Token expired");
            return BadRequest(new { error = ex.Message });
        }
        catch (TokenAlreadyUsedException ex)
        {
            _logger.LogWarning("Set password failed: Token already used");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("password"))
        {
            _logger.LogWarning("Set password failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during set password");
            return BadRequest(new { error = "Failed to set password. Please try again or request a new activation link." });
        }
    }

    /// <summary>
    /// Get current password policy configuration
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Password policy requirements</returns>
    /// <response code="200">Password policy retrieved successfully</response>
    [AllowAnonymous]
    [HttpGet("password-policy")]
    [ProducesResponseType(typeof(PasswordPolicyResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PasswordPolicyResponse>> GetPasswordPolicyAsync(
        CancellationToken cancellationToken)
    {
        var query = new GetPasswordPolicyQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    /// <param name="command">Login credentials (email and password)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JWT access token, refresh token, and user information</returns>
    /// <response code="200">Login successful - returns tokens and user info</response>
    /// <response code="401">Invalid email or password</response>
    /// <response code="403">Account not activated or password expired</response>
    /// <response code="429">Too many login attempts - rate limit exceeded</response>
    [AllowAnonymous]
    [HttpPost("login")]
    // [EnableRateLimiting("login")] // TEMPORARILY DISABLED FOR DEVELOPMENT
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LoginResponse>> LoginAsync(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            _logger.LogInformation("Login successful for user {UserId}", result.User.UserId);
            return Ok(result);
        }
        catch (InvalidCredentialsException ex)
        {
            _logger.LogWarning("Login failed: Invalid credentials for email {Email}", command.Email);
            return Unauthorized(new { error = ex.Message });
        }
        catch (AccountNotActivatedException ex)
        {
            _logger.LogWarning("Login failed: Account not activated for email {Email}", command.Email);
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email {Email}", command.Email);
            return BadRequest(new { error = "Login failed. Please try again." });
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="command">Refresh token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New access token and user information</returns>
    /// <response code="200">Token refreshed successfully</response>
    /// <response code="401">Invalid, expired, or revoked refresh token</response>
    [AllowAnonymous]
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> RefreshTokenAsync(
        [FromBody] RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            _logger.LogInformation("Token refreshed for user {UserId}", result.User.UserId);
            return Ok(result);
        }
        catch (InvalidTokenException ex)
        {
            _logger.LogWarning("Token refresh failed: {Message}", ex.Message);
            return Unauthorized(new { error = ex.Message });
        }
        catch (TokenExpiredException ex)
        {
            _logger.LogWarning("Token refresh failed: {Message}", ex.Message);
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return BadRequest(new { error = "Token refresh failed. Please login again." });
        }
    }

    /// <summary>
    /// Logout - revokes refresh token
    /// </summary>
    /// <param name="command">Refresh token to revoke</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Logout successful</response>
    [AllowAnonymous] // Accept both authenticated and unauthenticated requests
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> LogoutAsync(
        [FromBody] LogoutCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(command, cancellationToken);
            _logger.LogInformation("Logout successful");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            // Return success anyway (idempotent)
            return NoContent();
        }
    }

    /// <summary>
    /// Get current authenticated user's profile
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User profile information</returns>
    /// <response code="200">Profile retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">User not found</response>
    [Authorize]
    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> GetProfileAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetUserProfileQuery();
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Get profile failed: {Message}", ex.Message);
            return Unauthorized(new { error = ex.Message });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Get profile failed: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile");
            return BadRequest(new { error = "Failed to retrieve profile. Please try again." });
        }
    }

    /// <summary>
    /// Update current authenticated user's profile
    /// PESEL cannot be modified. Email changes require confirmation.
    /// </summary>
    /// <param name="command">Profile update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Update result with updated profile</returns>
    /// <response code="200">Profile updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="409">Email already in use by another account</response>
    [Authorize]
    [HttpPut("profile")]
    [ProducesResponseType(typeof(UpdateUserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UpdateUserProfileResponse>> UpdateProfileAsync(
        [FromBody] UpdateUserProfileCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            _logger.LogInformation("Profile updated successfully for user");
            return Ok(result);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Update profile failed: {Message}", ex.Message);
            return Unauthorized(new { error = ex.Message });
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning("Update profile failed: {Message}", ex.Message);
            return Conflict(new { error = ex.Message });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Update profile failed: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return BadRequest(new { error = "Failed to update profile. Please try again." });
        }
    }

    /// <summary>
    /// Confirm email change using token from confirmation email
    /// </summary>
    /// <param name="token">Email change confirmation token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Confirmation result</returns>
    /// <response code="200">Email changed successfully</response>
    /// <response code="400">Invalid, expired, or already used token</response>
    /// <response code="409">New email now taken by another user</response>
    [AllowAnonymous]
    [HttpGet("confirm-email-change")]
    [ProducesResponseType(typeof(ConfirmEmailChangeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ConfirmEmailChangeResponse>> ConfirmEmailChangeAsync(
        [FromQuery] string token,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new ConfirmEmailChangeCommand(token);
            var result = await _mediator.Send(command, cancellationToken);
            _logger.LogInformation("Email change confirmed successfully");
            return Ok(result);
        }
        catch (InvalidTokenException ex)
        {
            _logger.LogWarning("Email change confirmation failed: Invalid token");
            return BadRequest(new { error = ex.Message });
        }
        catch (TokenExpiredException ex)
        {
            _logger.LogWarning("Email change confirmation failed: Token expired");
            return BadRequest(new { error = ex.Message });
        }
        catch (TokenAlreadyUsedException ex)
        {
            _logger.LogWarning("Email change confirmation failed: Token already used");
            return BadRequest(new { error = ex.Message });
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning("Email change confirmation failed: {Message}", ex.Message);
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email change confirmation");
            return BadRequest(new { error = "Email change confirmation failed. Please try again or request a new email change." });
        }
    }
}

