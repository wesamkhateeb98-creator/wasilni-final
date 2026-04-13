using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Options;
using SoftPro.Wasilni.Presentation.ActionFilters.Authorization;
using System.Text;

namespace SoftPro.Wasilni.Presentation.Extensions;

public static class RegistrationExtensions
{
    public static IServiceCollection RegisterPresentation(this IServiceCollection services, IConfiguration configuration)
        => services
                .AddValidator()
                .RegisterAuthSwagger("My API")
                .RegisterOptions(configuration)
                .RegisterAuth(configuration)
                .RegisterCors()
                ;

    public static IServiceCollection AddValidator(this IServiceCollection services)
        => services.AddValidatorsFromAssemblyContaining<Program>();

    private static IServiceCollection RegisterAuthSwagger(this IServiceCollection services, string title)
    {
        return services.AddSwaggerGen(
            o =>
            {
                o.SwaggerDoc("v1", new OpenApiInfo() { Title = title, Version = "v1" });
                o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                o.AddSecurityRequirement(new OpenApiSecurityRequirement
              {
                  {
                      new OpenApiSecurityScheme
                      {
                          Reference = new OpenApiReference
                          {
                              Type = ReferenceType.SecurityScheme,
                              Id = "Bearer"
                          }
                      },
                      Array.Empty<string>()
                  }
            });
            });

    }

    public static IServiceCollection RegisterOptions(this IServiceCollection services, IConfiguration configuration)
    {
        return services.RegisterOption<JwtOption>(configuration, JwtOption.SectionNanme);
    }

    private static IServiceCollection RegisterAuth(this IServiceCollection services, IConfiguration configuration)
    {

        JwtOption jwtOption = services.GetOptions<JwtOption>();

        services.AddAuthentication(option =>
        {
            option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(
            op =>
            {
                op.RequireHttpsMetadata = false;
                op.SaveToken = false;
                op.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtOption.IssuerJwt,
                    ValidAudience = jwtOption.AudienceJwt,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOption.keyJwt)),
                    ClockSkew = TimeSpan.Zero,
                };
                // SignalR sends JWT via query string when using WebSockets
                op.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(token) &&
                            context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        services.AddAuthorization(options =>
        {
            foreach (var role in Enum.GetValues<Role>().Select(x => x.ToString()))
                options.AddPolicy(role, policy => policy.RequireRole(role));

            options.AddPolicy(HasBusAttribute.PolicyName,
                policy => policy.Requirements.Add(new HasBusRequirement()));

            foreach (var permission in Enum.GetValues<Permission>())
                options.AddPolicy(
                    $"{HasPermissionAttribute.PolicyPrefix}{permission}",
                    policy => policy.Requirements.Add(new HasPermissionRequirement(permission)));
        });

        services.AddSingleton<IAuthorizationHandler, HasBusAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, HasPermissionAuthorizationHandler>();
        return services;
    }

    public static IServiceCollection RegisterOption<T>(this IServiceCollection services, IConfiguration configuration, string sectionName) where T : class
    {
        var section = configuration.GetRequiredSection(sectionName);


        services.AddOptions<T>()
             .Bind(section)
             .ValidateDataAnnotations()
             .ValidateOnStart();

        return services;
    }

    public static T GetOptions<T>(this IServiceCollection services) where T : class
        => services.BuildServiceProvider().GetRequiredService<IOptions<T>>().Value;

    public static IServiceCollection RegisterCors(this IServiceCollection services)
        => services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.WithOrigins(
                          "http://localhost:4200",
                          "http://localhost:3000"
                      )
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();  // مطلوب لـ SignalR
            });
        });

}
