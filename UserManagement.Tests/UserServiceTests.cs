using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using UserManagement.Api.Data;
using UserManagement.Api.Exceptions;
using UserManagement.Api.Models.Requests;
using UserManagement.Api.Services;

namespace UserManagement.Tests;

public sealed class UserServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesAndReturnsUser()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var service = CreateService(context);

        var user = await service.CreateAsync(new CreateUserRequest("Ada Lovelace", " ADA@example.COM "));

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("Ada Lovelace", user.Name);
        Assert.Equal("ada@example.com", user.Email);
        Assert.True(user.IsActive);
        Assert.NotEqual(default, user.CreatedAt);
    }

    [Fact]
    public async Task CreateAsync_RejectsDuplicateEmailCaseInsensitively()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var service = CreateService(context);

        await service.CreateAsync(new CreateUserRequest("First User", "person@example.com"));

        await Assert.ThrowsAsync<DuplicateEmailException>(
            () => service.CreateAsync(new CreateUserRequest("Second User", " PERSON@example.COM ")));
    }

    [Fact]
    public async Task GetAllAsync_ExcludesInactiveUsers()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var service = CreateService(context);

        var activeUser = await service.CreateAsync(new CreateUserRequest("Active User", "active@example.com"));
        var inactiveUser = await service.CreateAsync(new CreateUserRequest("Inactive User", "inactive@example.com"));
        await service.DeactivateAsync(inactiveUser.Id);

        var result = await service.GetAllAsync(new UserQueryParameters());

        Assert.Contains(result.Items, user => user.Id == activeUser.Id);
        Assert.DoesNotContain(result.Items, user => user.Id == inactiveUser.Id);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByName()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var service = CreateService(context);

        await service.CreateAsync(new CreateUserRequest("Grace Hopper", "grace@example.com"));
        await service.CreateAsync(new CreateUserRequest("Margaret Hamilton", "margaret@example.com"));

        var result = await service.GetAllAsync(new UserQueryParameters { Search = "hopper" });

        var user = Assert.Single(result.Items);
        Assert.Equal("Grace Hopper", user.Name);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByEmail()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var service = CreateService(context);

        await service.CreateAsync(new CreateUserRequest("Grace Hopper", "grace@example.com"));
        await service.CreateAsync(new CreateUserRequest("Margaret Hamilton", "margaret@example.com"));

        var result = await service.GetAllAsync(new UserQueryParameters { Search = "MARGARET@EXAMPLE" });

        var user = Assert.Single(result.Items);
        Assert.Equal("margaret@example.com", user.Email);
    }

    [Fact]
    public async Task GetAllAsync_PaginatesCorrectly()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var service = CreateService(context);

        await service.CreateAsync(new CreateUserRequest("Alpha User", "alpha@example.com"));
        await service.CreateAsync(new CreateUserRequest("Bravo User", "bravo@example.com"));
        await service.CreateAsync(new CreateUserRequest("Charlie User", "charlie@example.com"));

        var result = await service.GetAllAsync(new UserQueryParameters { Page = 2, PageSize = 1 });

        var user = Assert.Single(result.Items);
        Assert.Equal("Bravo User", user.Name);
        Assert.Equal(2, result.Page);
        Assert.Equal(1, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public async Task GetAllAsync_ClampsPageSizeToMaximum()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var service = CreateService(context);

        var result = await service.GetAllAsync(
            new UserQueryParameters { PageSize = 500 });

        Assert.Equal(100, result.PageSize);
    }
    
    [Fact]
    public async Task GetByIdAsync_ReturnsActiveUser()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var service = CreateService(context);

        var created = await service.CreateAsync(new CreateUserRequest("Ada Lovelace", "ada@example.com"));

        var user = await service.GetByIdAsync(created.Id);

        Assert.NotNull(user);
        Assert.Equal(created.Id, user.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullForInactiveUser()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var service = CreateService(context);

        var created = await service.CreateAsync(new CreateUserRequest("Inactive User", "inactive@example.com"));
        await service.DeactivateAsync(created.Id);

        var user = await service.GetByIdAsync(created.Id);

        Assert.Null(user);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesNameAndEmail()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var service = CreateService(context);

        var created = await service.CreateAsync(new CreateUserRequest("Old Name", "old@example.com"));

        var updated = await service.UpdateAsync(
            created.Id,
            new UpdateUserRequest("New Name", " NEW@example.COM "));

        Assert.NotNull(updated);
        Assert.Equal("New Name", updated.Name);
        Assert.Equal("new@example.com", updated.Email);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_PermitsKeepingTheSameEmail()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var service = CreateService(context);

        var created = await service.CreateAsync(new CreateUserRequest("Ada Lovelace", "ada@example.com"));

        var updated = await service.UpdateAsync(
            created.Id,
            new UpdateUserRequest("Ada Byron", " ADA@example.COM "));

        Assert.NotNull(updated);
        Assert.Equal("Ada Byron", updated.Name);
        Assert.Equal("ada@example.com", updated.Email);
    }

    [Fact]
    public async Task UpdateAsync_RejectsEmailUsedByAnotherUser()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var service = CreateService(context);

        var first = await service.CreateAsync(new CreateUserRequest("First User", "first@example.com"));
        await service.CreateAsync(new CreateUserRequest("Second User", "second@example.com"));

        await Assert.ThrowsAsync<DuplicateEmailException>(
            () => service.UpdateAsync(first.Id, new UpdateUserRequest("First User", " SECOND@example.COM ")));
    }

    [Fact]
    public async Task DeactivateAsync_SetsIsActiveToFalse()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var service = CreateService(context);

        var created = await service.CreateAsync(new CreateUserRequest("Ada Lovelace", "ada@example.com"));

        var deactivated = await service.DeactivateAsync(created.Id);
        var user = await context.Users.SingleAsync(user => user.Id == created.Id);

        Assert.True(deactivated);
        Assert.False(user.IsActive);
    }

    [Fact]
    public async Task DeactivateAsync_ReturnsFalseForMissingUser()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var service = CreateService(context);

        var deactivated = await service.DeactivateAsync(Guid.NewGuid());

        Assert.False(deactivated);
    }

    private static async Task<SqliteConnection> CreateOpenConnectionAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        return connection;
    }

    private static async Task<ApplicationDbContext> CreateContextAsync(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new ApplicationDbContext(options);
        await context.Database.MigrateAsync();
        return context;
    }

    private static UserService CreateService(ApplicationDbContext context)
    {
        return new UserService(context, NullLogger<UserService>.Instance);
    }
}
