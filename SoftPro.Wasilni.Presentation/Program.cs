using Microsoft.AspNetCore.SignalR;
using SoftPro.Wasilni.Application.Extensions;
using SoftPro.Wasilni.Infrastructure.Extensions;
using SoftPro.Wasilni.Presentation.ActionFilters;
using SoftPro.Wasilni.Presentation.Extensions;
using SoftPro.Wasilni.Presentation.Filters;
using SoftPro.Wasilni.Presentation.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(x =>
{
    x.Filters.Add<ValidatorActionFilter>();
});

builder.Services.AddSignalR(o => o.EnableDetailedErrors = true)
                .AddHubOptions<SoftPro.Wasilni.Presentation.Hubs.TrackingHub>(o =>
                {
                    o.AddFilter<HubExceptionFilter>();
                });

builder.Services.AddMemoryCache();

builder.Services
    .RegisterApplication()
    .RegisterInfrastructure(builder.Configuration)
    .RegisterPresentation(builder.Configuration)
    /*.RegisterFirebase(builder.Configuration)*/;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<CultureInfoManager>();


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
    c.RoutePrefix = "swagger";
});
//}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();


app.MapControllers();
app.MapHub<SoftPro.Wasilni.Presentation.Hubs.TrackingHub>("/hubs/tracking");

app.Run();

public partial class Program { }