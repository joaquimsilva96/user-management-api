using FluentValidation;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Data;
using UserManagement.Api.Infrastructure.Exceptions;
using UserManagement.Api.Services;
using UserManagement.Api.Validators;

if (args.Contains("--healthcheck", StringComparer.OrdinalIgnoreCase))
{
    using var httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(3)
    };

    try
    {
        using var response = await httpClient.GetAsync(
            "http://localhost:8080/health");

        return response.IsSuccessStatusCode ? 0 : 1;
    }
    catch
    {
        return 1;
    }
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

await app.ApplyDatabaseMigrationsAsync();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

if (app.Configuration.GetValue<bool>("Http:UseHttpsRedirection"))
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));
app.MapControllers();

app.Run();

return 0;

public partial class Program;
