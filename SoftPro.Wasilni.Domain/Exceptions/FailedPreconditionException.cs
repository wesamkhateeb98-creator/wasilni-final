using SoftPro.Wasilni.Domain.Exceptions.Abstraction;

namespace SoftPro.Wasilni.Domain.Exceptions;

public class FailedPreconditionException(string message) : Exception, IProblemDetailsProvider
{
    public ServiceProblemDetails GetProblemDetails()
        => new ServiceProblemDetails
        {
            Title = message,
            Type = "Failed Precondition",
        };
}
