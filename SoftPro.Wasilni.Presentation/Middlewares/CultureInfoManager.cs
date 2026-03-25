using System.Globalization;

namespace SoftPro.Wasilni.Presentation.Middlewares;

public class CultureInfoManager(RequestDelegate requestDelegate)
{
    private readonly RequestDelegate _requestDelegate = requestDelegate;

    public async Task Invoke(HttpContext context)
    {
        string userLangs = context.Request.Headers["language"].ToString();
        string? firstLang = userLangs.Split(',').FirstOrDefault();
        string culture = firstLang switch
        {
            "ar" => firstLang,
            _ => "en",
        };
        CultureInfo.CurrentCulture = new CultureInfo(culture);
        CultureInfo.CurrentUICulture = new CultureInfo(culture);

        await _requestDelegate(context);
    }
}
