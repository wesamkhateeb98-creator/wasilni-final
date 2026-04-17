using System.Net;
using SoftPro.Wasilni.Domain.Exceptions.Abstraction;

namespace SoftPro.Wasilni.Domain.Exceptions;

public class NotFoundException(string message) : Exception, IProblemDetailsProvider
{
    public ServiceProblemDetails GetProblemDetails()
        => new ServiceProblemDetails
        {
            Title = message,
            Type = "Not Found",
            StatusCode = HttpStatusCode.NotFound,
        };
}
