using FluentValidation;
using Microsoft.AspNetCore.SignalR;

namespace SoftPro.Wasilni.Presentation.ActionFilters.Hub;

public class HubValidationFilter(IServiceProvider serviceProvider) : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext context,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        foreach (var arg in context.HubMethodArguments)
        {
            if (arg is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());
            var validator     = serviceProvider.GetService(validatorType) as IValidator;

            if (validator is null) continue;

            var validationContext = new ValidationContext<object>(arg);
            var result            = await validator.ValidateAsync(validationContext);

            if (!result.IsValid)
                throw new ValidationException(result.Errors);
        }

        return await next(context);
    }
}
