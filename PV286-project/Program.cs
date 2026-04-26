using BusinessLayer.Cli.Commands.Decode;
using BusinessLayer.Cli.Commands.Verify;
using BusinessLayer.Cli.Commands;
using BusinessLayer.Cli.Commands.Encode;
using BusinessLayer.Cli.Commands.Help;
using BusinessLayer.Cli.Parser;
using BusinessLayer.Services;
using BusinessLayer.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

#if RELEASE
using Microsoft.Extensions.Logging;
#endif

namespace PV286_project
{
    internal static class Program
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
                        services.AddSingleton<IEncodeService, EncodeService>();
                        services.AddSingleton<IDecodeService, DecodeService>();
                        services.AddSingleton<IVerifyService, VerifyService>();


                        services.AddSingleton<ICliParser, EncodeParser>();
                        services.AddSingleton<ICliParser, DecodeParser>();
                        services.AddSingleton<ICliParser, VerifyParser>();
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
