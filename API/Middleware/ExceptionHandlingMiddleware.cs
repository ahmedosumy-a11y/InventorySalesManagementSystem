using System.Net;
using System.Text.Json;
using FluentValidation;
using InventorySalesManagementSystem.Application.Interfaces;
using InventorySalesManagementSystem.Domain.Enums;

namespace InventorySalesManagementSystem.API.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    ISlackService slackService)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException exception)
        {
            logger.LogInformation("Validation failed: {Message}", exception.Message);

            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var response = new
            {
                message = "Validation failed.",
                errors = exception.Errors.Select(x => x.ErrorMessage).ToList()
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (AppException exception)
        {
            logger.LogInformation("Application error: {Message}", exception.Message);

            context.Response.StatusCode = (int)exception.StatusCode;
            context.Response.ContentType = "application/json";

            var response = new
            {
                message = exception.Message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception occurred.");

            // 🚨 Slack notification (ONLY real system errors)
            await slackService.SendAsync(
                SlackChannelType.Errors,
                "Unhandled Exception",
                $"""
                Error: {exception.Message}
                Path: {context.Request.Path}
                Method: {context.Request.Method}
                Time: {DateTime.UtcNow}
                """
            );

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                message = "An unexpected server error occurred."
            }));
        }
    }
}































// using System.Net;
// using System.Text.Json;
// using FluentValidation;

// namespace InventorySalesManagementSystem.API.Middleware;

// public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
// {
//     public async Task InvokeAsync(HttpContext context)
//     {
//         try
//         {
//             await next(context);
//         }
//         catch (ValidationException exception)
//         {
//             logger.LogWarning(exception, "Validation error occurred.");
//             context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
//             context.Response.ContentType = "application/json";

//             var response = new
//             {
//                 message = "Validation failed.",
//                 errors = exception.Errors.Select(x => x.ErrorMessage).ToList()
//             };

//             await context.Response.WriteAsync(JsonSerializer.Serialize(response));
//         }
//         catch (AppException exception)
//         {
//             logger.LogWarning(exception, "Application error occurred.");
//             context.Response.StatusCode = (int)exception.StatusCode;
//             context.Response.ContentType = "application/json";

//             await context.Response.WriteAsync(JsonSerializer.Serialize(new
//             {
//                 message = exception.Message
//             }));
//         }
//         catch (Exception exception)
//         {
//             logger.LogError(exception, "Unhandled exception occurred.");
//             context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
//             context.Response.ContentType = "application/json";

//             await context.Response.WriteAsync(JsonSerializer.Serialize(new
//             {
//                 message = "An unexpected server error occurred."
//             }));
//         }
//     }
// }
