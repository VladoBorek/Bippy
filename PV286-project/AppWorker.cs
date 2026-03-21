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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var args = consoleArgs.args;

            bool success =
                args.Length > 0 && args[0] == "batch"
                    ? await RunBatchAsync(args, stoppingToken)
                    : Run(args);

            Environment.ExitCode = success ? 0 : 1;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Application was cancelled");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Application terminated unexpectedly");
            Environment.ExitCode = 1;
        }
        finally
        {
            lifetime.StopApplication();
        }
    }

    private async Task<bool> RunBatchAsync(string[] args, CancellationToken stoppingToken)
    {
        if (args.Length < 2)
        {
            await Console.Error.WriteLineAsync("Usage: batch <filepath|->");
            return false;
        }

        var source = args[1];
        bool interactive = source == "-";

        TextReader reader = interactive ? Console.In : new StreamReader(source);

        bool allSucceeded = true;

        try
        {
            if (interactive)
                Console.WriteLine(
                    "Batch interactive mode. Enter args separated by ' ', one invocation per line. EOF to exit."
                );

            string? line;
            while (
                !stoppingToken.IsCancellationRequested
                && (line = await reader.ReadLineAsync(stoppingToken)) != null
            )
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var lineArgs = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                bool lineSuccess = Run(lineArgs);
                if (!lineSuccess)
                {
                    allSucceeded = false;
                    await Console.Error.WriteLineAsync($"Line failed: {line}");
                }

                Console.WriteLine(Environment.NewLine);

                if (interactive)
                {
                    await Console.Out.FlushAsync();
                }
            }
        }
        finally
        {
            if (!interactive)
                reader.Dispose();
        }

        return allSucceeded;
    }

    private bool Run(string[] args)
    {
        var parsedCommandRes = argParser.Parse(args);
        if (parsedCommandRes.IsFailed)
        {
            Console.Error.WriteLine(parsedCommandRes.Error);
            return false;
        }

        return parsedCommandRes.Value.Handle();
    }
}
