using Domain.Resources;
using SoftPro.Wasilni.Domain.Exceptions;
using SoftPro.Wasilni.Domain.Exceptions.Abstraction;
using System.Net;
using System.Text.Json;

namespace SoftPro.Wasilni.Presentation.Middlewares
{
    public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                logger.LogWarning("*****************************Request");
                await _next(context);
            }
            catch (Exception ex) when (ex is IProblemDetailsProvider provider)
            {
                await WriteError(context, provider);
            }
            catch (Exception ex) when (
                ex.InnerException?.Message.Contains("DELETE") == true &&
                ex.InnerException?.Message.Contains("REFERENCE constraint") == true)
            {
                var recordInUseMessage = Phrases.RecordInUse;
                await WriteError(context, new FailedPreconditionException(recordInUseMessage));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                await WriteGenericError(context, ex.Message);
            }
        }

        private static async Task WriteError(HttpContext context, IProblemDetailsProvider provider)
        {
            var problemDetails = provider.GetProblemDetails();
            var statusCode = problemDetails.Type switch
            {
                "Not Found" => HttpStatusCode.NotFound,
                "Already Exists" => HttpStatusCode.Conflict,
                "Forbidden" => HttpStatusCode.Forbidden,
                "Unauthorization" => HttpStatusCode.Unauthorized,
                "Invalid Arguement" => HttpStatusCode.BadRequest,
                "Failed Precondition" => HttpStatusCode.PreconditionFailed,
                "Too Many Requests" => HttpStatusCode.TooManyRequests,
                _ => HttpStatusCode.InternalServerError
            };

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var response = new
            {
                title = problemDetails.Title,
                type = problemDetails.Type,
                status = (int)statusCode,
                detail = problemDetails.Detail,
                instance = problemDetails.Instance,
                extensions = problemDetails.Extensions.Count > 0 ? problemDetails.Extensions : null
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            }));
        }

        private static async Task WriteGenericError(HttpContext context, string message)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var errorTitle = Title.Error;

            var response = new
            {
                title = errorTitle,
                type = "Internal Server Error",
                status = 500,
                detail = message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }
    }
}

