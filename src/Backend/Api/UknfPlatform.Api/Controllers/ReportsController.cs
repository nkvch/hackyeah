using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UknfPlatform.Application.Communication.Reports.Commands;

namespace UknfPlatform.Api.Controllers;

/// <summary>
/// Reports controller for regulatory report submission and management
/// </summary>
[ApiController]
[Route("api/reports")]
[Authorize] // All endpoints require authentication
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IMediator mediator, ILogger<ReportsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Submit a regulatory report by uploading an XLSX file
    /// </summary>
    /// <param name="command">Report submission data (multipart/form-data)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Report submission result with report ID and initial status</returns>
    /// <response code="201">Report submitted successfully</response>
    /// <response code="400">Invalid request data (wrong file type, format errors)</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User lacks Reporting permission for the entity</response>
    /// <response code="404">Entity not found</response>
    /// <response code="409">Duplicate report (same entity, type, period already exists)</response>
    /// <response code="413">File too large (exceeds 100 MB limit)</response>
    /// <remarks>
    /// Requires "communication.reports.submit" permission for the specified entity.
    /// 
    /// Accepted file format: XLSX only (Excel OpenXML format).
    /// Maximum file size: 100 MB.
    /// 
    /// The system validates the file content (magic bytes) to prevent malicious files.
    /// After successful upload, the report enters "Working" status and is queued for validation.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/reports/upload
    ///     Content-Type: multipart/form-data
    ///     
    ///     entityId: 1001
    ///     reportType: Quarterly
    ///     reportingPeriod: Q1_2025
    ///     file: [binary XLSX file]
    /// 
    /// </remarks>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(104_857_600)] // 100 MB limit
    [ProducesResponseType(typeof(SubmitReportResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    public async Task<ActionResult<SubmitReportResponse>> SubmitReportAsync(
        [FromForm] SubmitReportCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Processing report submission: Entity {EntityId}, Type {ReportType}, Period {ReportingPeriod}",
                command.EntityId, command.ReportType, command.ReportingPeriod);

            var result = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation(
                "Report submitted successfully. ReportId: {ReportId}, Status: {Status}",
                result.ReportId, result.Status);

            // Return 201 Created with Location header
            return CreatedAtAction(
                actionName: null, // We don't have GetReportById endpoint yet (will be in Story 4.5)
                routeValues: new { id = result.ReportId },
                value: result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(
                "Report submission forbidden: {Message}. Entity: {EntityId}",
                ex.Message, command.EntityId);
            return Forbid(); // 403 Forbidden
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning("Entity not found: {Message}", ex.Message);
            return NotFound(new { error = ex.Message }); // 404 Not Found
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            _logger.LogWarning("Duplicate report: {Message}", ex.Message);
            return Conflict(new { error = ex.Message }); // 409 Conflict
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("storage"))
        {
            _logger.LogError("File storage error: {Message}", ex.Message);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "Failed to store file. Please try again." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during report submission");
            return BadRequest(new { error = "Report submission failed. Please check your file and try again." });
        }
    }

    // TODO: Additional endpoints will be added in other stories:
    // - GET /api/reports - List reports (Story 4.5)
    // - GET /api/reports/{id} - Get report details (Story 4.5)
    // - GET /api/reports/{id}/download - Download report file (Story 4.5)
    // - POST /api/reports/{id}/contest - Contest report (Story 4.4)
    // - POST /api/reports/{id}/correction - Submit correction (Story 4.6)
}

