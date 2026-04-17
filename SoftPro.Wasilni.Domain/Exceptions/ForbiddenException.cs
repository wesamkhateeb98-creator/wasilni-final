using System.Net;
using SoftPro.Wasilni.Domain.Exceptions.Abstraction;

namespace SoftPro.Wasilni.Domain.Exceptions;

public class ForbiddenException(string message) : Exception, IProblemDetailsProvider
{
    public ServiceProblemDetails GetProblemDetails()
        => new ServiceProblemDetails
        {
            Title = message,
            Type = "Forbidden",
            StatusCode = HttpStatusCode.Forbidden,
        };
}
