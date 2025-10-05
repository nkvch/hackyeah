using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UknfPlatform.Application.Auth.AccessRequests.DTOs;
using UknfPlatform.Application.Auth.AccessRequests.Queries;

namespace UknfPlatform.Api.Controllers;

/// <summary>
/// API endpoints for managing access requests.
/// Story 2.1: Automatic Access Request Creation
/// Epic 2: Authorization & Access Requests
/// </summary>
[ApiController]
[Route("api/access-requests")]
[Authorize]
public class AccessRequestsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AccessRequestsController> _logger;

    public AccessRequestsController(IMediator mediator, ILogger<AccessRequestsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current user's access request.
    /// </summary>
    /// <returns>Access request details with pre-populated user data</returns>
    /// <response code="200">Returns the user's access request</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="404">No access request found for user</response>
    [HttpGet("my-request")]
    [ProducesResponseType(typeof(AccessRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyAccessRequest()
    {
        try
        {
            var query = new GetAccessRequestQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving access request");
            return StatusCode(500, new { error = "An error occurred while retrieving your access request" });
        }
    }
}

