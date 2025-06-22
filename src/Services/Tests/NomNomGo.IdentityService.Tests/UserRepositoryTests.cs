// UserRepositoryTests.cs
using Microsoft.EntityFrameworkCore;
using NomNomGo.IdentityService.Domain.Entities;
using NomNomGo.IdentityService.Domain.Entities.Relationships;
using NomNomGo.IdentityService.Infrastructure.Persistence.Database;
using NomNomGo.IdentityService.Infrastructure.Repositories;

namespace NomNomGo.IdentityService.Tests;

public class UserRepositoryTests
{
    private readonly DbContextOptions<ApplicationDbContext> _contextOptions;

    public UserRepositoryTests()
    {
        _contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private ApplicationDbContext CreateContext()
    {
        var context = new ApplicationDbContext(_contextOptions);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task GetByEmailAsync_UserExists_ReturnsUser()
    {
        using var context = CreateContext();
        var email = "test@example.com";
        context.Users.Add(new User { Id = Guid.NewGuid(), Email = email, Username = "user1", PasswordHash = "hash" });
        await context.SaveChangesAsync();
        var repo = new UserRepository(context);
        var user = await repo.GetByEmailAsync(email);
        Assert.NotNull(user);
    }

    [Fact]
    public async Task GetByEmailAsync_CaseInsensitive_ReturnsUser()
    {
        using var context = CreateContext();
        var email = "test@example.com";
        context.Users.Add(new User { Id = Guid.NewGuid(), Email = email.ToLower(), Username = "user1", PasswordHash = "hash" });
        await context.SaveChangesAsync();
        var repo = new UserRepository(context);
        var user = await repo.GetByEmailAsync("TEST@EXAMPLE.COM");
        Assert.NotNull(user);
    }

    [Fact]
    public async Task GetByUsernameAsync_UserExists_ReturnsUser()
    {
        using var context = CreateContext();
        var username = "testuser";
        context.Users.Add(new User { Id = Guid.NewGuid(), Username = username, Email = "email@test.com", PasswordHash = "hash" });
        await context.SaveChangesAsync();
        var repo = new UserRepository(context);
        var user = await repo.GetByUsernameAsync(username);
        Assert.Equal(username, user.Username);
    }

    [Fact]
    public async Task GetWithRolesAsync_UserWithRoles_ReturnsUserWithRoles()
    {
        using var context = CreateContext();
        var userId = Guid.NewGuid();
        var role = new Role { Id = Guid.NewGuid(), Name = "Admin" };
        var user = new User { Id = userId, Username = "testuser", Email = "t@e.com", PasswordHash = "hash" };
        var userRole = new UserRole { UserId = userId, RoleId = role.Id, User = user, Role = role };
        context.Users.Add(user);
        context.Roles.Add(role);
        context.UserRoles.Add(userRole);
        await context.SaveChangesAsync();
        var repo = new UserRepository(context);
        var result = await repo.GetWithRolesAsync(userId);
        Assert.Contains(result.UserRoles, ur => ur.RoleId == role.Id);
    }
}
