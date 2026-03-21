using BusinessLayer.CLI.Parser;
using BusinessLayer.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PV286_project;

// BackgroundService is the standard base class for IHostedService in console apps.
// The host calls StartAsync → ExecuteAsync. When ExecuteAsync returns, the host shuts down.
public class AppWorker : BackgroundService
{
    private readonly ILogger<AppWorker> logger;
    private readonly IHostApplicationLifetime lifetime;
    private readonly ConsoleArgs consoleArgs;
    private readonly IArgParser argParser;

    public AppWorker(
        ILogger<AppWorker> logger,
        IHostApplicationLifetime lifetime,
        ConsoleArgs consoleArgs,
        IArgParser argParser
    )
    {
        this.logger = logger;
        this.lifetime = lifetime;
        this.consoleArgs = consoleArgs;
        this.argParser = argParser;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var parsedCommandRes = argParser.Parse(consoleArgs.args);
            if (parsedCommandRes.IsFailed)
            {
                Console.Error.WriteLine(parsedCommandRes.Error);
                Environment.Exit(1);
                return Task.CompletedTask;
            }

            var handledCommandRes = parsedCommandRes.Value.Handle();
            if (!handledCommandRes)
            {
                Environment.Exit(1);
                return Task.CompletedTask;
            }

            Environment.Exit(0);
            return Task.CompletedTask;
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

        return Task.CompletedTask;
    }
}
