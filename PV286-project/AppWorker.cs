using System.Data;
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
    private readonly IArgParser parser;

    public AppWorker(
        ILogger<AppWorker> logger,
        IHostApplicationLifetime lifetime,
        ConsoleArgs consoleArgs,
        IArgParser parser
    )
    {
        this.logger = logger;
        this.lifetime = lifetime;
        this.consoleArgs = consoleArgs;
        this.parser = parser;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var parseRes = parser.Parse(consoleArgs.args);
            if (parseRes.IsFailed)
            {
                Console.Error.WriteLine(string.Join(", ", parseRes.Errors.Select(e => e.Message)));
                Environment.Exit(1);
                return Task.CompletedTask;
            }

            var handleRes = parseRes.Value.Handle();
            if (!handleRes)
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
