using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Api.Exceptions;

namespace UserManagement.Api.Infrastructure.Exceptions;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            DuplicateEmailException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Duplicate email",
                Detail = exception.Message
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred."
            }
        };

        if (exception is not DuplicateEmailException)
        {
            logger.LogError(exception, "Unhandled exception.");
        }

        httpContext.Response.StatusCode = problemDetails.Status.GetValueOrDefault(
            StatusCodes.Status500InternalServerError);

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }
}
