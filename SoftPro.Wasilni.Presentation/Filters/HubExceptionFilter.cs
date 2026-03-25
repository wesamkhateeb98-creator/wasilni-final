using FluentValidation;
using Microsoft.AspNetCore.SignalR;
using SoftPro.Wasilni.Domain.Exceptions.Abstraction;
using System.Text.Json;

namespace SoftPro.Wasilni.Presentation.Filters;

public class HubExceptionFilter : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext context,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        try
        {
            return await next(context);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors
                .Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
                .ToList();

            throw new HubException(JsonSerializer.Serialize(new
            {
                type   = "Validation Error",
                detail = "One or more validation errors occurred.",
                errors
            }));
        }
        catch (Exception ex) when (ex is IProblemDetailsProvider provider)
        {
            var details = provider.GetProblemDetails();
            throw new HubException(JsonSerializer.Serialize(new
            {
                type   = details.Type,
                detail = details.Detail
            }));
        }
        catch (HubException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new HubException(JsonSerializer.Serialize(new
            {
                type   = "Internal Server Error",
                detail = ex.Message
            }));
        }
    }
}
