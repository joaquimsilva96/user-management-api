using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Api.Models.Requests;
using UserManagement.Api.Models.Responses;

namespace UserManagement.Tests;

public sealed class UsersControllerIntegrationTests
{
    [Fact]
    public async Task Post_CreatesUserAndReturnsCreated()
    {
        using var factory = new UserManagementApiFactory();
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/users",
            new CreateUserRequest("Ada Lovelace", "ada@example.com"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var user = await response.Content.ReadFromJsonAsync<UserResponse>();

        Assert.NotNull(user);
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.EndsWith($"/api/users/{user.Id}", response.Headers.Location!.ToString());
        Assert.Equal("ada@example.com", user.Email);
    }

    [Fact]
    public async Task Post_InvalidEmailReturnsValidationDetails()
    {
        using var factory = new UserManagementApiFactory();
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/users",
            new CreateUserRequest("Ada Lovelace", "not-an-email"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Equal("Validation failed", problem.Title);
        Assert.Contains(nameof(CreateUserRequest.Email), problem.Errors.Keys);
    }

    [Fact]
    public async Task Post_DuplicateEmailReturnsConflict()
    {
        using var factory = new UserManagementApiFactory();
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();

        await CreateUserAsync(client, "First User", "person@example.com");

        var response = await client.PostAsJsonAsync(
            "/api/users",
            new CreateUserRequest("Second User", " PERSON@example.COM "));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(409, problem.Status);
    }

    [Fact]
    public async Task Get_ReturnsPagedResult()
    {
        using var factory = new UserManagementApiFactory();
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();

        await CreateUserAsync(client, "Alpha User", "alpha@example.com");
        await CreateUserAsync(client, "Bravo User", "bravo@example.com");

        var result = await client.GetFromJsonAsync<PagedResult<UserResponse>>(
            "/api/users?page=1&pageSize=1");

        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(1, result.Page);
        Assert.Equal(1, result.PageSize);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task GetById_ReturnsUser()
    {
        using var factory = new UserManagementApiFactory();
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();
        var created = await CreateUserAsync(client, "Ada Lovelace", "ada@example.com");

        var user = await client.GetFromJsonAsync<UserResponse>($"/api/users/{created.Id}");

        Assert.NotNull(user);
        Assert.Equal(created.Id, user.Id);
    }

    [Fact]
    public async Task GetById_MissingIdReturnsNotFound()
    {
        using var factory = new UserManagementApiFactory();
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_UpdatesUser()
    {
        using var factory = new UserManagementApiFactory();
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();
        var created = await CreateUserAsync(client, "Old Name", "old@example.com");

        var response = await client.PutAsJsonAsync(
            $"/api/users/{created.Id}",
            new UpdateUserRequest("New Name", " NEW@example.COM "));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<UserResponse>();

        Assert.NotNull(updated);
        Assert.Equal("New Name", updated.Name);
        Assert.Equal("new@example.com", updated.Email);
    }

    [Fact]
    public async Task Put_DuplicateEmailReturnsConflict()
    {
        using var factory = new UserManagementApiFactory();
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();
        var first = await CreateUserAsync(client, "First User", "first@example.com");
        await CreateUserAsync(client, "Second User", "second@example.com");

        var response = await client.PutAsJsonAsync(
            $"/api/users/{first.Id}",
            new UpdateUserRequest("First User", " SECOND@example.COM "));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNoContentAndSubsequentGetReturnsNotFound()
    {
        using var factory = new UserManagementApiFactory();
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();
        var created = await CreateUserAsync(client, "Ada Lovelace", "ada@example.com");

        var deleteResponse = await client.DeleteAsync($"/api/users/{created.Id}");
        var getResponse = await client.GetAsync($"/api/users/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Theory]
    [InlineData("/api/users?page=0&pageSize=10")]
    [InlineData("/api/users?page=1&pageSize=101")]
    public async Task Get_InvalidPaginationReturnsValidationDetails(string url)
    {
        using var factory = new UserManagementApiFactory();
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.NotEmpty(problem.Errors);
    }

    private static async Task<UserResponse> CreateUserAsync(
        HttpClient client,
        string name,
        string email)
    {
        var response = await client.PostAsJsonAsync(
            "/api/users",
            new CreateUserRequest(name, email));
        response.EnsureSuccessStatusCode();

        var user = await response.Content.ReadFromJsonAsync<UserResponse>();

        return user ?? throw new InvalidOperationException("Expected user response.");
    }
}
