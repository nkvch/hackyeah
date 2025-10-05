using Microsoft.EntityFrameworkCore;
using UknfPlatform.Domain.Auth.Entities;
using UknfPlatform.Domain.Auth.Enums;
using UknfPlatform.Domain.Auth.Repositories;
using UknfPlatform.Infrastructure.Persistence.Contexts;

namespace UknfPlatform.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for AccessRequest entity.
/// Story 2.1: Automatic Access Request Creation
/// </summary>
public class AccessRequestRepository : IAccessRequestRepository
{
    private readonly ApplicationDbContext _context;

    public AccessRequestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AccessRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AccessRequests
            .Include(ar => ar.User)
            .Include(ar => ar.ReviewedByUser)
            .FirstOrDefaultAsync(ar => ar.Id == id, cancellationToken);
    }

    public async Task<AccessRequest?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.AccessRequests
            .Include(ar => ar.User)
            .Include(ar => ar.ReviewedByUser)
            .FirstOrDefaultAsync(ar => ar.UserId == userId, cancellationToken);
    }

    public async Task<AccessRequest?> GetActiveRequestByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Get the most recent request that's not blocked
        return await _context.AccessRequests
            .Include(ar => ar.User)
            .Include(ar => ar.ReviewedByUser)
            .Where(ar => ar.UserId == userId && ar.Status != AccessRequestStatus.Blocked)
            .OrderByDescending(ar => ar.CreatedDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<AccessRequest>> GetAllByStatusAsync(AccessRequestStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.AccessRequests
            .Include(ar => ar.User)
            .Include(ar => ar.ReviewedByUser)
            .Where(ar => ar.Status == status)
            .OrderByDescending(ar => ar.SubmittedDate ?? ar.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AccessRequest>> GetAllVisibleToReviewersAsync(CancellationToken cancellationToken = default)
    {
        // All requests except "Working" status
        return await _context.AccessRequests
            .Include(ar => ar.User)
            .Include(ar => ar.ReviewedByUser)
            .Where(ar => ar.Status != AccessRequestStatus.Working)
            .OrderByDescending(ar => ar.SubmittedDate ?? ar.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(AccessRequest request, CancellationToken cancellationToken = default)
    {
        await _context.AccessRequests.AddAsync(request, cancellationToken);
    }

    public Task UpdateAsync(AccessRequest request, CancellationToken cancellationToken = default)
    {
        _context.AccessRequests.Update(request);
        return Task.CompletedTask;
    }
}

