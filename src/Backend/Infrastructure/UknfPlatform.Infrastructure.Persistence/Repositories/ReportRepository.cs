using Microsoft.EntityFrameworkCore;
using UknfPlatform.Domain.Communication.Entities;
using UknfPlatform.Domain.Communication.Enums;
using UknfPlatform.Domain.Communication.Interfaces;
using UknfPlatform.Infrastructure.Persistence.Contexts;

namespace UknfPlatform.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Report entity
/// </summary>
public class ReportRepository : IReportRepository
{
    private readonly ApplicationDbContext _context;

    public ReportRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reports
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Report>> GetByEntityIdAsync(
        long entityId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Reports
            .AsNoTracking()
            .Where(r => r.EntityId == entityId)
            .OrderByDescending(r => r.SubmittedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Report>> GetByStatusAsync(
        ValidationStatus status, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Reports
            .AsNoTracking()
            .Where(r => r.ValidationStatus == status)
            .OrderByDescending(r => r.SubmittedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Report report, CancellationToken cancellationToken = default)
    {
        await _context.Reports.AddAsync(report, cancellationToken);
    }

    public async Task UpdateAsync(Report report, CancellationToken cancellationToken = default)
    {
        _context.Reports.Update(report);
        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(
        long entityId, 
        string reportType, 
        string reportingPeriod, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Reports
            .AsNoTracking()
            .AnyAsync(r => 
                r.EntityId == entityId && 
                r.ReportType == reportType && 
                r.ReportingPeriod == reportingPeriod &&
                !r.IsArchived, 
                cancellationToken);
    }

    public async Task<IEnumerable<Report>> GetTimeoutReportsAsync(CancellationToken cancellationToken = default)
    {
        // Reports that have been in Transmitted or Ongoing status for more than 24 hours
        var timeoutThreshold = DateTime.UtcNow.AddHours(-24);

        return await _context.Reports
            .AsNoTracking()
            .Where(r => 
                (r.ValidationStatus == ValidationStatus.Transmitted || 
                 r.ValidationStatus == ValidationStatus.Ongoing) &&
                r.ValidationStartedDate.HasValue &&
                r.ValidationStartedDate.Value < timeoutThreshold)
            .ToListAsync(cancellationToken);
    }
}

