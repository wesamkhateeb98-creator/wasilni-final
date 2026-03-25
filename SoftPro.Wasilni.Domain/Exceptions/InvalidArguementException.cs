using SoftPro.Wasilni.Domain.Exceptions.Abstraction;

namespace SoftPro.Wasilni.Domain.Exceptions;

public class InvalidArguementException(List<(string, string)> value) : Exception, IProblemDetailsProvider
{
    public ServiceProblemDetails GetProblemDetails()
        => new ServiceProblemDetails
        {
            Title = "",
            Type = "Invalid Arguement",
            Extensions = value.ToDictionary(x => x.Item1, x => (object?)x.Item2)
        };
}
