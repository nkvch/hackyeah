using Microsoft.EntityFrameworkCore;
using UknfPlatform.Domain.Auth.Entities;
using UknfPlatform.Domain.Communication.Entities;

namespace UknfPlatform.Infrastructure.Persistence.Contexts;

/// <summary>
/// Main database context for the UKNF Communication Platform
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Auth Domain
    public DbSet<User> Users => Set<User>();
    public DbSet<ActivationToken> ActivationTokens => Set<ActivationToken>();
    public DbSet<EmailChangeToken> EmailChangeTokens => Set<EmailChangeToken>();
    public DbSet<PasswordHistory> PasswordHistories => Set<PasswordHistory>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuthenticationAuditLog> AuthenticationAuditLogs => Set<AuthenticationAuditLog>();
    public DbSet<AccessRequest> AccessRequests => Set<AccessRequest>(); // Story 2.1

    // Communication Domain
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ValidationResult> ValidationResults => Set<ValidationResult>();
    public DbSet<Message> Messages => Set<Message>(); // Story 5.1
    public DbSet<MessageAttachment> MessageAttachments => Set<MessageAttachment>(); // Story 5.1
    public DbSet<MessageRecipient> MessageRecipients => Set<MessageRecipient>(); // Story 5.1

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}

