using System;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UnicornHack.Data;
using UnicornHack.Hubs;
using UnicornHack.Services;
using UnicornHack.Services.English;

namespace UnicornHack
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddDbContext<GameDbContext>(options =>
                    options.EnableSensitiveDataLogging()
                        .UseSqlServer(Configuration.GetConnectionString(name: "DefaultConnection")));

            services.AddMvc();

            services.AddSignalR();

            services.AddSingleton<ILanguageService, EnglishLanguageService>();
            services.AddSingleton<GameServices>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        // Override default exception page for json requests
                        var error = context.Features.Get<IExceptionHandlerFeature>();
                        if (error != null)
                            //&& context.Request.Path == "/PerformAction")
                        {
                            context.Response.StatusCode = 200;
                            context.Response.ContentType = "text/html";
                            await context.Response.WriteAsync("<html><body><br>Error<br>" + Environment.NewLine);

                            await context.Response.WriteAsync(
                                ReplaceNewLines(HtmlEncoder.Default.Encode(error.Error.ToString())) +
                                "<br>" + Environment.NewLine);

                            await context.Response.WriteAsync("</body></html>" + Environment.NewLine);
                        }
                        else
                        {
                            throw new Exception();
                        }
                        //ExceptionDispatchInfo.Capture(error.Error).Throw();
                    });
                });

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
                        context.Database.EnsureDeleted();
                        context.Database.EnsureCreated();
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
                routes.MapHub<GameHub>("gameHub");
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

        private string ReplaceNewLines(string text)
        {
            var newLine = HtmlEncoder.Default.Encode(Environment.NewLine);
            return "<p>" + text
                       .Replace(newLine + newLine, "</p><p>")
                       .Replace(newLine, Environment.NewLine + "<br />" + Environment.NewLine)
                       .Replace("</p><p>", "</p>" + Environment.NewLine + "<p>") + "</p>";
        }
    }
}
