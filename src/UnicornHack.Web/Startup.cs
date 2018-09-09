using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UnicornHack.Data;
using UnicornHack.Hubs;
using UnicornHack.Services;
using UnicornHack.Services.English;

namespace UnicornHack
{
    // TODO: Separate into UnicornHack.Storage and UnicornHack.Web
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILanguageService, EnglishLanguageService>();
            services.AddSingleton<GameServices>();
            services.AddSingleton<GameTransmissionProtocol>();

            services.AddEntityFrameworkSqlServer();
            // Workaround for #13087
            // TODO: AddDbContextPool
            services.AddDbContext<GameDbContext>((p, options) =>
                options.UseSqlServer(Configuration.GetConnectionString(name: "DefaultConnection"))
                    .UseInternalServiceProvider(p)
                    .EnableSensitiveDataLogging()
                    .ConfigureWarnings(w => w.Default(WarningBehavior.Throw)
                        .Log(CoreEventId.SensitiveDataLoggingEnabledWarning)
                        .Log(RelationalEventId.QueryClientEvaluationWarning)));

            services.AddMvc();

            services.AddSignalR(o => o.EnableDetailedErrors = true).AddMessagePackProtocol();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseDatabaseErrorPage();

                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = false,
                    ReactHotModuleReplacement = false
                });

                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                    .CreateScope())
                {
                    using (var context = serviceScope.ServiceProvider.GetService<GameDbContext>())
                    {
                        new DatabaseCleaner().Clean(context.Database);
                    }
                }
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");

                try
                {
                    using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                        .CreateScope())
                    {
                        serviceScope.ServiceProvider.GetService<GameDbContext>()
                            .Database.Migrate();
                    }
                }
                catch
                {
                }
            }

            app.UseStaticFiles();

            app.UseSignalR(routes =>
            {
                routes.MapHub<GameHub>("/gameHub");
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "game",
                    template: "Game",
                    defaults: new { controller = "Home", action = "Game" });

                routes.MapRoute(
                    name: "getState",
                    template: "GetState",
                    defaults: new { controller = "Home", action = "GetState" });

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
