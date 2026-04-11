using MessagePack;
using MessagePack.Resolvers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using UnicornHack.Data;
using UnicornHack.Hubs;
using UnicornHack.Services;
using UnicornHack.Services.English;

namespace UnicornHack;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var services = builder.Services;
        var configuration = builder.Configuration;
        var env = builder.Environment;

        services.AddSingleton<ILanguageService, EnglishLanguageService>();
        services.AddSingleton<GameServices>();
        services.AddSingleton<GameTransmissionProtocol>();
        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddDbContextPool<GameDbContext>((_, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString(name: "DefaultConnection")!)
                .ConfigureWarnings(w => w.Default(WarningBehavior.Throw)
                    .Log(CoreEventId.SensitiveDataLoggingEnabledWarning));

            if (env.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
            }
        });

        services.AddControllersWithViews();

        // TODO: Add a sensible policy for production
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

        services.AddSignalR(o =>
        {
            o.EnableDetailedErrors = true;
            o.DisableImplicitFromServicesParameters = true;
        })
            .AddMessagePackProtocol(
                o => o.SerializerOptions = MessagePackSerializerOptions.Standard
                    .WithSecurity(MessagePackSecurity.UntrustedData)
                    .WithResolver(CompositeResolver.Create(
                        BuiltinResolver.Instance,
                        GeneratedResolver.Instance)));

        if (!env.IsDevelopment())
        {
            // TODO: Add connection string and other configuration for Application Insights
            services.AddApplicationInsightsTelemetry();
        }

        var app = builder.Build();

        if (env.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
            app.UseDeveloperExceptionPage();
            app.UseMigrationsEndPoint();

            using var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<GameDbContext>()!;
            new DatabaseCleaner().Clean(context.Database);
        }
        else
        {
            app.UseExceptionHandler("/Error");

            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles(new StaticFileOptions { ServeUnknownFileTypes = true });

        app.UseCors();
        //app.UseAuthorization();

        app.MapHub<GameHub>("/gameHub");
        app.MapControllers();

        app.Run();
    }
}
