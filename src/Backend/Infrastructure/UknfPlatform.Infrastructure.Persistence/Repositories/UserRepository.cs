using Microsoft.EntityFrameworkCore;
using UknfPlatform.Domain.Auth.Entities;
using UknfPlatform.Domain.Auth.Interfaces;
using UknfPlatform.Infrastructure.Persistence.Contexts;

namespace UknfPlatform.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for User entity
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByPeselAsync(string peselEncrypted, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.PeselEncrypted == peselEncrypted, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsByPeselAsync(string peselEncrypted, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.PeselEncrypted == peselEncrypted, cancellationToken);
    }

    public async Task<List<User>> GetAvailableRecipientsAsync(Guid currentUserId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.Id != currentUserId && u.IsActive)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
