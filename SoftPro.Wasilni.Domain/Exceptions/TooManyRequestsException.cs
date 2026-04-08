using SoftPro.Wasilni.Domain.Exceptions.Abstraction;

namespace SoftPro.Wasilni.Domain.Exceptions;

public class TooManyRequestsException : Exception, IProblemDetailsProvider
{
    private readonly string _message;

    public TooManyRequestsException(string message) : base(message)
        => _message = message;

    public ServiceProblemDetails GetProblemDetails()
        => new ServiceProblemDetails
        {
            Title  = _message,
            Detail = _message,
            Type   = "Too Many Requests",
        };
}
