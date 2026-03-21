using BusinessLayer.CLI.Parser;
using BusinessLayer.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PV286_project;

// BackgroundService is the standard base class for IHostedService in console apps.
// The host calls StartAsync → ExecuteAsync. When ExecuteAsync returns, the host shuts down.
public class AppWorker : BackgroundService
{
    private const string StdinDelimiter = "|";

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
        bool isStdin = source == "-";

        IEnumerable<string> invocations;

        if (isStdin)
        {
            var rest = string.Join(" ", args.Skip(2));
            invocations = rest.Split(StdinDelimiter, StringSplitOptions.RemoveEmptyEntries);
        }
        else
        {
            var lines = await File.ReadAllLinesAsync(source, stoppingToken);
            invocations = lines.Where(l => !string.IsNullOrWhiteSpace(l));
        }

        bool allSucceeded = true;

        foreach (var invocation in invocations)
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            var lineArgs = invocation.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (lineArgs.Length == 0)
                continue;

            bool lineSuccess = Run(lineArgs);
            if (!lineSuccess)
            {
                allSucceeded = false;
                await Console.Error.WriteLineAsync($"Invocation failed: {invocation.Trim()}");
            }
            Console.WriteLine(Environment.NewLine);
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
