
using Domain.Resources;
using SoftPro.Wasilni.Domain.Exceptions;
using System.Security.Claims;

namespace SoftPro.Wasilni.Presentation.Extensions;

public static class HelperExtensions
{
    public static int GetId(this ClaimsPrincipal claim)
    {
        Claim claimId = claim.FindFirst(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedException(Phrases.LoginAgain);
        return int.Parse(claimId.Value);
    }

    public static bool GetEnums<T>(int x) where T :Enum     
    {
        return Enum.IsDefined(typeof(T), x);
    }
}
