using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Data;
using UserManagement.Api.Entities;
using UserManagement.Api.Exceptions;
using UserManagement.Api.Models.Requests;
using UserManagement.Api.Models.Responses;

namespace UserManagement.Api.Services;

public sealed class UserService(
    ApplicationDbContext dbContext,
    ILogger<UserService> logger) : IUserService
{
    public async Task<UserResponse> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);

        if (await EmailExistsAsync(normalizedEmail, excludedUserId: null, cancellationToken))
        {
            throw new DuplicateEmailException();
        }

        var user = new User
        {
            Name = request.Name,
            Email = normalizedEmail
        };

        await dbContext.Users.AddAsync(user, cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            logger.LogInformation("Duplicate email was rejected while creating a user.");
            throw new DuplicateEmailException();
        }

        logger.LogInformation("Created user {UserId}.", user.Id);
        return MapToResponse(user);
    }

    public async Task<PagedResult<UserResponse>> GetAllAsync(
        UserQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(parameters.Page, 1);
        var pageSize = Math.Clamp(parameters.PageSize, 1, 100);

        var query = dbContext.Users
            .AsNoTracking()
            .Where(user => user.IsActive);

        var search = parameters.Search?.Trim().ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(user =>
                user.Name.ToLower().Contains(search) ||
                user.Email.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(user => user.Name)
            .ThenBy(user => user.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new UserResponse(
                user.Id,
                user.Name,
                user.Email,
                user.IsActive,
                user.CreatedAt,
                user.UpdatedAt))
            .ToListAsync(cancellationToken);

        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<UserResponse>(items, page, pageSize, totalCount, totalPages);
    }

    public async Task<UserResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == id && user.IsActive)
            .Select(user => new UserResponse(
                user.Id,
                user.Name,
                user.Email,
                user.IsActive,
                user.CreatedAt,
                user.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<UserResponse?> UpdateAsync(
        Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .SingleOrDefaultAsync(user => user.Id == id && user.IsActive, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var normalizedEmail = NormalizeEmail(request.Email);

        if (await EmailExistsAsync(normalizedEmail, id, cancellationToken))
        {
            throw new DuplicateEmailException();
        }

        user.Name = request.Name;
        user.Email = normalizedEmail;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            logger.LogInformation("Duplicate email was rejected while updating user {UserId}.", id);
            throw new DuplicateEmailException();
        }

        logger.LogInformation("Updated user {UserId}.", user.Id);
        return MapToResponse(user);
    }

    public async Task<bool> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .SingleOrDefaultAsync(user => user.Id == id && user.IsActive, cancellationToken);

        if (user is null)
        {
            return false;
        }

        user.IsActive = false;
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Deactivated user {UserId}.", id);
        return true;
    }

    private Task<bool> EmailExistsAsync(
        string normalizedEmail,
        Guid? excludedUserId,
        CancellationToken cancellationToken)
    {
        return dbContext.Users.AnyAsync(
            user =>
                user.Email == normalizedEmail &&
                (!excludedUserId.HasValue || user.Id != excludedUserId.Value),
            cancellationToken);
    }

    private static UserResponse MapToResponse(User user)
    {
        return new UserResponse(
            user.Id,
            user.Name,
            user.Email,
            user.IsActive,
            user.CreatedAt,
            user.UpdatedAt);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 };
    }
}
