using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UnicornHack
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
            => new WebHostBuilder()
                .UseApplicationInsights()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration(
                    (hostingContext, config) =>
                    {
                        IHostingEnvironment hostingEnvironment = hostingContext.HostingEnvironment;
                        config
                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", optional: true,
                                reloadOnChange: true);
                        if (hostingEnvironment.IsDevelopment())
                        {
                            Assembly assembly = Assembly.Load(new AssemblyName(hostingEnvironment.ApplicationName));
                            if (assembly != null)
                            {
                                config.AddUserSecrets(assembly, true);
                            }
                        }
                        config.AddEnvironmentVariables();
                        if (args == null)
                        {
                            return;
                        }
                        config.AddCommandLine(args);
                    })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                })
                .UseIISIntegration().UseDefaultServiceProvider((context, options)
                    => options.ValidateScopes = context.HostingEnvironment.IsDevelopment())
                .UseStartup<Startup>()
                .Build();
    }
}