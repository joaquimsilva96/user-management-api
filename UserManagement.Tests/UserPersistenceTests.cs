using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Data;
using UserManagement.Api.Entities;

namespace UserManagement.Tests;

public sealed class UserPersistenceTests
{
    [Fact]
    public async Task NewUser_ReceivesApplicationDefaults()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        var user = new User
        {
            Name = "Ada Lovelace",
            Email = "ada@example.com"
        };

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.True(user.IsActive);
        Assert.NotEqual(default, user.CreatedAt);
        Assert.Null(user.UpdatedAt);
    }

    [Fact]
    public async Task UpdatingUser_PopulatesUpdatedAt()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        var user = new User
        {
            Name = "Grace Hopper",
            Email = "grace@example.com"
        };

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        user.Name = "Rear Admiral Grace Hopper";
        await context.SaveChangesAsync();

        Assert.NotNull(user.UpdatedAt);
        Assert.True(user.UpdatedAt >= user.CreatedAt);
    }

    [Fact]
    public async Task DuplicateNormalizedEmails_AreRejectedByUniqueConstraint()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        await context.Users.AddAsync(new User
        {
            Name = "First User",
            Email = "person@example.com"
        });
        await context.SaveChangesAsync();

        await context.Users.AddAsync(new User
        {
            Name = "Second User",
            Email = " PERSON@example.com "
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task Email_IsTrimmedAndLowercasedBeforeSaving()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);

        var user = new User
        {
            Name = "Margaret Hamilton",
            Email = " MARGARET@example.COM "
        };

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        Assert.Equal("margaret@example.com", user.Email);
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
}
