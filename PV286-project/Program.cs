using BusinessLayer.Services;
using BusinessLayer.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PV286_project.Cli;
using PV286_project.Cli.Handlers;
using PV286_project.Cli.Interfaces;



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
                .ConfigureServices(
                    (context, services) =>
                    {
                        // context.Configuration gives access to appsettings.json values
                        // context.HostingEnvironment exposes DOTNET_ENVIRONMENT

                        // Register ConsoleArgs class for passing args to the rest of the application
                        services.AddSingleton(new ConsoleArgs { args = args });

                        // Business Layer
                        // Register application services
                        services.AddSingleton<IMnemonicService, MnemonicService>();

                        // CLI orchestration
                        services.AddSingleton<CliParser>();
                        services.AddSingleton<ICommandDispatcher, CommandDispatcher>();

                        // Command handlers
                        services.AddSingleton<EncodeCommandHandler>();
                        services.AddSingleton<HelpCommandHandler>();

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
