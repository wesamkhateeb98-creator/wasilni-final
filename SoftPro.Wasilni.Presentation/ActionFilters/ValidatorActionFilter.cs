using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SoftPro.Wasilni.Domain.Exceptions;


namespace SoftPro.Wasilni.Presentation.ActionFilters;

public class ValidatorActionFilter(IServiceProvider serviceProvider) : IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var argument in context.ActionArguments)
        {
            if (argument.Value is null)
                continue;

            var inputType = argument.Value.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(inputType);
            IValidator? validator;

            try
            {
                validator = serviceProvider.GetService(validatorType) as IValidator;
            }
            catch
            {
                continue;
            }

            if (validator is null)
                continue;

            var validationContextType = typeof(ValidationContext<>).MakeGenericType(inputType);
            var validationContext = Activator.CreateInstance(validationContextType, argument.Value);
            if (validationContext is null)
                continue;

            var validationResult = validator.Validate((IValidationContext)validationContext);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

                context.Result = new BadRequestObjectResult(new
                {
                    title = "Validation Error",
                    type = "Invalid Arguement",
                    status = StatusCodes.Status400BadRequest,
                    detail = "One or more validation errors occurred.",
                    extensions = errors
                });

                return;
            }
        }
    }
}
