using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoftPro.Wasilni.Application.Abstracts;
using SoftPro.Wasilni.Application.Abstracts.Repositories;
using SoftPro.Wasilni.Infrastructure.Persistence;
using SoftPro.Wasilni.Infrastructure.Repositories;


namespace SoftPro.Wasilni.Infrastructure.Extensions;

public static class RegistrationExtensions
{
    public static IServiceCollection RegisterInfrastructure(this IServiceCollection services, IConfiguration configuration)
        => services
            .RegisterDatabase(configuration)
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddHttpClient()
            .AddScoped<IWhatsAppRepository, WhatsAppRepository>();

    private static IServiceCollection RegisterDatabase(this IServiceCollection services, IConfiguration configuration)
        => services.AddDbContext<AppDbContext>(
                 options => options.UseSqlServer(configuration.GetConnectionString("Database")))
            .AddHostedService<DatabaseMigrationHostedService>();
}
