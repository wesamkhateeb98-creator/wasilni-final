using SoftPro.Wasilni.Domain.Entities;
using System.Globalization;
using System.Security.Claims;

namespace SoftPro.Wasilni.Application.Extensions;

public static class MapperExtensions
{
    public static List<Claim> GetClaim(this AccountEntity account)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new(ClaimTypes.MobilePhone,    account.PhoneNumber),
            new(ClaimTypes.Role,           account.Role.ToString()),
        };

        if (account.Permission.HasValue)
            claims.Add(new("Permission", account.Permission.Value.ToString()));

        return claims;
    }

    public static bool IsArabic => CultureInfo.CurrentCulture.Name == "ar";
}
