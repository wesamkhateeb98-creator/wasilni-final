using Microsoft.Extensions.DependencyInjection;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Application.Services;
using SoftPro.Wasilni.Domain.Helper;

namespace SoftPro.Wasilni.Application.Extensions;

public static class RegistrationExtensions
{
    public static IServiceCollection RegisterApplication(this IServiceCollection services)
        => services.AddScoped<IAccountService, AccountService>()
                    .AddScoped<IBusService, BusService>()
        //            .AddScoped<ICityService, CityService>()
                    .AddScoped<ILineService, LineService>()
                    .AddScoped<ITripService, TripService>()
                    .AddScoped<AuthHelper>();
}
