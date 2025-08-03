using MessagePack.Resolvers;
using MessagePack;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using UnicornHack.Data;
using UnicornHack.Hubs;
using UnicornHack.Services;
using UnicornHack.Services.English;

namespace UnicornHack;

public class Startup
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        _configuration = configuration;
        _env = env;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ILanguageService, EnglishLanguageService>();
        services.AddSingleton<GameServices>();
        services.AddSingleton<GameTransmissionProtocol>();
        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddDbContextPool<GameDbContext>((_, options) =>
        {
            options.UseSqlServer(_configuration.GetConnectionString(name: "DefaultConnection")!)
                .ConfigureWarnings(w => w.Default(WarningBehavior.Throw)
                    .Log(CoreEventId.SensitiveDataLoggingEnabledWarning));

            if (_env.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
            }
        });

        services.AddControllersWithViews();
        //services.AddRazorPages();

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

        services.AddApplicationInsightsTelemetry();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            //app.UseWebAssemblyDebugging();
            app.UseDeveloperExceptionPage();
            app.UseMigrationsEndPoint();

            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
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

        app.UseRouting();

        app.UseCors();
        //app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<GameHub>("/gameHub");

            endpoints.MapControllers();
        });
    }
}
