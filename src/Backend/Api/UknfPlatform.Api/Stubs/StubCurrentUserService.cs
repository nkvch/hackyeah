using UknfPlatform.Application.Shared.Interfaces;

namespace UknfPlatform.Api.Stubs;

/// <summary>
/// Stub implementation of ICurrentUserService for development
/// TODO: Replace with proper implementation using HttpContext and JWT claims
/// </summary>
public class StubCurrentUserService : ICurrentUserService
{
    // Hardcoded test user for development
    public Guid UserId => Guid.Parse("00000000-0000-0000-0000-000000000001");
    public string Email => "test.user@example.com";
    public string FullName => "Test User";
    public string? Phone => "+48123456789";
    public bool IsAuthenticated => true;

    public Task<bool> HasPermissionAsync(string permission, long? entityId = null, CancellationToken cancellationToken = default)
    {
        // For MVP, grant all permissions
        // TODO: Implement proper permission checking when Epic 2 is complete
        return Task.FromResult(true);
    }
}

