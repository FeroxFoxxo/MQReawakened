using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Server.Base.Core.Abstractions;
using Server.Web.Abstractions;
using Server.Web.Middleware;

namespace Server.Web;

public class Web(ILogger<Web> logger) : WebModule(logger)
{
    public override void AddServices(IServiceCollection services, Module[] modules)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "MQR API", Version = "v1" }));
        services.AddRazorPages();
    }

    public override void InitializeWeb(WebApplicationBuilder builder)
    {
        builder.WebHost.CaptureStartupErrors(true);
        builder.Services.AddMemoryCache();
    }

    public override void PostWebBuild(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "MQR API V1");
                c.RoutePrefix = "api-docs"; // Custom route for Swagger UI
            });

            app.UseMiddleware<RequestMiddleware>();

            app.UseHsts();
        }

        app.UseDefaultFiles();
        app.UseStaticFiles(new StaticFileOptions()
        {
            ServeUnknownFileTypes = true
        });

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages();
        app.MapControllers();
    }
}
