using Microsoft.Extensions.DependencyInjection;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Domain.Auth.Entities;
using UknfPlatform.Domain.Auth.Enums;
using UknfPlatform.Infrastructure.Persistence.Contexts;

namespace UknfPlatform.Tests.Integration.Api.Auth;

public class IntegrationTestBase : IClassFixture<IntegrationTestWebApplicationFactory>
{
    protected readonly IntegrationTestWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    public IntegrationTestBase(IntegrationTestWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected async Task<User> CreateUserAsync(string email)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = User.Create(
            email,
            "Test",
            "User",
            "12345678901",
            UserType.ExternalUser
        );

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return user;
    }

    protected async Task<User> CreateActivatedUserWithPasswordAsync(string email, string password)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var user = User.Create(
            email,
            "Test",
            "User",
            "12345678901",
            UserType.ExternalUser
        );

        user.Activate();
        var hashedPassword = passwordHasher.HashPassword(password);
        user.SetPassword(hashedPassword);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return user;
    }

    protected async Task SaveChangesAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.SaveChangesAsync();
    }
}

