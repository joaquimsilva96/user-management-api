namespace UserManagement.Api.Models.Responses;

public sealed record UserResponse(
    Guid Id,
    string Name,
    string Email,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
