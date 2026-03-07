using BusinessLayer.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PV286_project;

// BackgroundService is the standard base class for IHostedService in console apps.
// The host calls StartAsync → ExecuteAsync. When ExecuteAsync returns, the host shuts down.
public class AppWorker : BackgroundService
{
    private readonly IGreetService greeterService;
    private readonly ILogger<AppWorker> logger;
    private readonly IHostApplicationLifetime lifetime;

    public AppWorker(
        IGreetService greeterService,
        ILogger<AppWorker> logger,
        IHostApplicationLifetime lifetime
    )
    {
        this.greeterService = greeterService;
        this.logger = logger;
        this.lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation("Application starting");

            var greeting = greeterService.Greet("World");
            Console.WriteLine(greeting);

            logger.LogInformation("Application finished successfully");
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown via Ctrl+C — not an error
            logger.LogWarning("Application was cancelled");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Application terminated unexpectedly");
            Environment.ExitCode = 1;
        }
        finally
        {
            // Signal the host to stop after work is done
            lifetime.StopApplication();
        }
    }
}
