using Microsoft.EntityFrameworkCore;
using UknfPlatform.Domain.Auth.Entities;
using UknfPlatform.Domain.Auth.Interfaces;
using UknfPlatform.Infrastructure.Persistence.Contexts;

namespace UknfPlatform.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for AuthenticationAuditLog entity
/// </summary>
public class AuthenticationAuditLogRepository : IAuthenticationAuditLogRepository
{
    private readonly ApplicationDbContext _context;

    public AuthenticationAuditLogRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(AuthenticationAuditLog auditLog, CancellationToken cancellationToken = default)
    {
        if (auditLog == null)
            throw new ArgumentNullException(nameof(auditLog));

        await _context.AuthenticationAuditLogs.AddAsync(auditLog, cancellationToken);
    }

    public async Task<IEnumerable<AuthenticationAuditLog>> GetRecentAttemptsByEmailAsync(
        string email,
        DateTime since,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Enumerable.Empty<AuthenticationAuditLog>();

        return await _context.AuthenticationAuditLogs
            .Where(aal => aal.Email.ToLower() == email.ToLower() && aal.Timestamp >= since)
            .OrderByDescending(aal => aal.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuthenticationAuditLog>> GetRecentAttemptsByIpAsync(
        string ipAddress,
        DateTime since,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return Enumerable.Empty<AuthenticationAuditLog>();

        return await _context.AuthenticationAuditLogs
            .Where(aal => aal.IpAddress == ipAddress && aal.Timestamp >= since)
            .OrderByDescending(aal => aal.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuthenticationAuditLog>> GetUserAuthenticationHistoryAsync(
        Guid userId,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuthenticationAuditLogs
            .Where(aal => aal.UserId == userId)
            .OrderByDescending(aal => aal.Timestamp)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}

