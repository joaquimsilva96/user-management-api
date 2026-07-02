namespace UserManagement.Api.Models.Requests;

public sealed record UserQueryParameters
{
    public string? Search { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}
