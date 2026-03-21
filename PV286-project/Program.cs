using BusinessLayer.Services;
using BusinessLayer.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security.Cryptography;

void Test()
{
    using var md5 = MD5.Create(); // SCS should warn about weak hash
}

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

                        // Register application services
                        services.AddSingleton<IGreetService, GreeterService>();
                        services.AddSingleton<IMnemonicService, MnemonicService>();

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
