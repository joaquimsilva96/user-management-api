using UserManagement.Api.Models.Requests;
using UserManagement.Api.Models.Responses;

namespace UserManagement.Api.Services;

public interface IUserService
{
    Task<UserResponse> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedResult<UserResponse>> GetAllAsync(
        UserQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<UserResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<UserResponse?> UpdateAsync(
        Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
