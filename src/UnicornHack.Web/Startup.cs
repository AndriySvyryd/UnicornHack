using System;
using System.Runtime.ExceptionServices;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnicornHack.Models;
using UnicornHack.Services;
using UnicornHack.Services.English;

namespace UnicornHack
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile(path: "appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                //builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString(name: "DefaultConnection"))
                    .EnableSensitiveDataLogging())
                .AddDbContext<GameDbContext>(options =>
                    options.EnableSensitiveDataLogging()
                        .UseSqlServer(
                            Configuration.GetConnectionString(name: "DefaultConnection"),
                            b => b.MaxBatchSize(maxBatchSize: 128)));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();

            services.AddSignalR(options => options.Hubs.EnableDetailedErrors = true);

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddSingleton<ILanguageService, EnglishLanguageService>();
            services.AddSingleton<GameServices>();
            services.AddMemoryCache();
        }

        private string ReplaceNewLines(string text)
        {
            var newLine = HtmlEncoder.Default.Encode(Environment.NewLine);
            return "<p>" + text
                .Replace(newLine + newLine, "</p><p>")
                .Replace(newLine, Environment.NewLine + "<br />" + Environment.NewLine)
                .Replace("</p><p>", "</p>" + Environment.NewLine + "<p>") + "</p>";
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection(key: "Logging"));
            loggerFactory.AddDebug();

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
                            //&& context.Request.Path == "/Home/PerformAction")
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
                            if (error == null)
                            {
                                throw new Exception();
                            }
                            ExceptionDispatchInfo.Capture(error.Error).Throw();
                        }
                    });
                });

                app.UseDatabaseErrorPage();
                //app.UseBrowserLink();

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
                app.UseExceptionHandler(errorHandlingPath: "/Home/Error");

                try
                {
                    using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                        .CreateScope())
                    {
                        serviceScope.ServiceProvider.GetService<ApplicationDbContext>()
                            .Database.Migrate();
                        serviceScope.ServiceProvider.GetService<GameDbContext>()
                            .Database.Migrate();
                    }
                }
                catch
                {
                }
            }

            app.UseStaticFiles();

            app.UseIdentity();

            app.UseSignalR();

            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}