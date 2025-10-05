using Microsoft.EntityFrameworkCore;
using UknfPlatform.Domain.Auth.Entities;
using UknfPlatform.Domain.Auth.Interfaces;
using UknfPlatform.Infrastructure.Persistence.Contexts;

namespace UknfPlatform.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for PasswordHistory entity
/// </summary>
public class PasswordHistoryRepository : IPasswordHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public PasswordHistoryRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<PasswordHistory>> GetRecentPasswordsAsync(Guid userId, int count, CancellationToken cancellationToken = default)
    {
        return await _context.PasswordHistories
            .Where(ph => ph.UserId == userId)
            .OrderByDescending(ph => ph.CreatedDate)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(PasswordHistory passwordHistory, CancellationToken cancellationToken = default)
    {
        await _context.PasswordHistories.AddAsync(passwordHistory, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}

