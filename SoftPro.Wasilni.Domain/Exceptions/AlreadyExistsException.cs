using System.Net;
using SoftPro.Wasilni.Domain.Exceptions.Abstraction;

namespace SoftPro.Wasilni.Domain.Exceptions;

public class AlreadyExistsException(string message) : Exception, IProblemDetailsProvider
{
    public ServiceProblemDetails GetProblemDetails()
        => new ServiceProblemDetails
        {
            Title = message,
            Type = "Already Exists",
            StatusCode = HttpStatusCode.Conflict,
        };
}
