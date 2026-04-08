using BusinessLayer.CLI.Commands;
using BusinessLayer.CLI.Commands.Encode;
using BusinessLayer.CLI.Commands.Help;
using BusinessLayer.CLI.Parser;
using BusinessLayer.Services;
using BusinessLayer.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PV286_project
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Host.CreateDefaultBuilder does the following automatically:
            //   - Loads appsettings.json and appsettings.{Environment}.json
            //   - Configures console + debug logging providers
            //   - Sets up DI container (IServiceProvider)
            IHost host = Host.CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
#if RELEASE
         logging.ClearProviders();
#endif
                })
                .ConfigureServices(
                    (context, services) =>
                    {
                        // context.Configuration gives access to appsettings.json values
                        // context.HostingEnvironment exposes DOTNET_ENVIRONMENT

                        // Register ConsoleArgs class for passing args to the rest of the application
                        services.AddSingleton(new ConsoleArgs { args = args });

                        // Business Layer
                        // Register application services
                        services.AddSingleton<ICommandService, CommandService>();
                        services.AddSingleton<ICliParser, EncodeParser>();
                        services.AddSingleton<ICliParser, HelpParser>();
                        services.AddSingleton<IArgParser, ArgParser>();

                        // AppWorker is the application's main entry point via IHostedService.
                        // The host starts it, awaits its completion, then shuts down cleanly.
                        services.AddHostedService<AppWorker>();
                    }
                )
                .Build();

            await host.RunAsync();
        }
    }
}
