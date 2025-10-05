using UknfPlatform.Domain.Auth.Entities;
using UknfPlatform.Domain.Auth.Enums;

namespace UknfPlatform.Domain.Auth.Repositories;

/// <summary>
/// Repository for managing access requests.
/// Story 2.1: Automatic Access Request Creation
/// </summary>
public interface IAccessRequestRepository
{
    Task<AccessRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<AccessRequest?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<AccessRequest?> GetActiveRequestByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<List<AccessRequest>> GetAllByStatusAsync(AccessRequestStatus status, CancellationToken cancellationToken = default);
    
    Task<List<AccessRequest>> GetAllVisibleToReviewersAsync(CancellationToken cancellationToken = default);
    
    Task AddAsync(AccessRequest request, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(AccessRequest request, CancellationToken cancellationToken = default);
}

