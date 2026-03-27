using System;
using SmartTasks.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace SmartTasks.Api.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        // This ProblemDetails object will be the foundation for our response
        var problemDetails = new ProblemDetails
        {
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
        };

        // Map exceptions to status codes and titles
        (problemDetails.Status, problemDetails.Title, problemDetails.Detail) = exception switch
        {
            DomainException domainEx =>
                ((int)HttpStatusCode.BadRequest, "Bad Request", domainEx.Message),

            ValidationException validationEx =>
                ((int)HttpStatusCode.BadRequest, "Validation Error", "One or more validation errors occurred."),

            UnauthorizedAccessException unauthorizedEx =>
                ((int)HttpStatusCode.Unauthorized, "Unauthorized", unauthorizedEx.Message),

            _ =>
                ((int)HttpStatusCode.InternalServerError, "An internal server error has occurred.", "Please try again later.")
        };

        // For validation exceptions, we can add the specific errors to the extensions
        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors
                .Select(e => new { property = e.PropertyName, error = e.ErrorMessage });
        }

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        // Return true to signal that the exception has been handled.
        return true;
    }
}
