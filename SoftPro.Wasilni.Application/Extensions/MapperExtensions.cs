using SoftPro.Wasilni.Domain.Entities;
using System.Globalization;
using System.Security.Claims;

namespace SoftPro.Wasilni.Application.Extensions;

public static class MapperExtensions
{
    public static List<Claim> GetClaim(this AccountEntity account)
        => [
                new(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new(ClaimTypes.MobilePhone, account.PhoneNumber.ToString()),
                new(ClaimTypes.Role, account.Role.ToString()),
            ];


    public static bool IsArabic => CultureInfo.CurrentCulture.Name == "ar";


}
