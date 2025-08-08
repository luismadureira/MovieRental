using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

public class GlobalExceptionHandler
{
    public static async Task HandleException(HttpContext context)
    {
        IExceptionHandlerPathFeature? exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        Exception? exception = exceptionHandlerPathFeature?.Error;

        ILogger<Program> logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        string correlationId = Guid.NewGuid().ToString(); // Generate a new correlation ID for the request
        logger.LogError(exception, "Unhandled exception. Correlation ID: {CorrelationId}", correlationId);

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/problem+json";

        if (exception is ArgumentException)
        {
            // Use Problem Details
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://example.com/errors/unexpected-error",
                title = "Invalid input provided.",
                status = 400, // Bad Request
                detail = exception?.Message,
                instance = context.Request.Path
            });
        }
        else if (exception is DbUpdateException)
        {
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://example.com/errors/unexpected-error",
                title = "A database error occurred.",
                status = 500, // Internal Server Error
                detail = exception?.Message,
                instance = context.Request.Path
            });
        }
        else
        {
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://example.com/errors/unexpected-error",
                title = "An unexpected error occurred. Please try again later.",
                status = 500, // Internal Server Error
                detail = exception?.Message,
                instance = context.Request.Path
            });
        }
    }
}