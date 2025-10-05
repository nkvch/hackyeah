using UknfPlatform.Domain.Communication.Entities;

namespace UknfPlatform.Domain.Communication.Interfaces;

/// <summary>
/// Repository interface for Report entity
/// </summary>
public interface IReportRepository
{
    /// <summary>
    /// Gets a report by ID
    /// </summary>
    Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets reports for a specific entity
    /// </summary>
    Task<IEnumerable<Report>> GetByEntityIdAsync(long entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets reports by validation status
    /// </summary>
    Task<IEnumerable<Report>> GetByStatusAsync(
        Enums.ValidationStatus status, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new report
    /// </summary>
    Task AddAsync(Report report, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing report
    /// </summary>
    Task UpdateAsync(Report report, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a report exists for entity and period
    /// </summary>
    Task<bool> ExistsAsync(
        long entityId, 
        string reportType, 
        string reportingPeriod, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets reports that have exceeded validation timeout (24 hours)
    /// </summary>
    Task<IEnumerable<Report>> GetTimeoutReportsAsync(CancellationToken cancellationToken = default);
}

