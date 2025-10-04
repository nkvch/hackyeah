using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UknfPlatform.Application.Auth.Authentication.Commands;

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
    [AllowAnonymous]
    [HttpPost("resend-activation")]
    [ProducesResponseType(typeof(ResendActivationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
}

