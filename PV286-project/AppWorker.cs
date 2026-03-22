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
            var isBatch = consoleArgs.args.Length > 0 && consoleArgs.args[0] == "batch";
            bool success;

            if (!isBatch)
            {
                success = Run(consoleArgs.args);
            }
            else
            {
                success = await RunBatchAsync();
            }

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

    private async Task<bool> RunBatchAsync()
    {
        bool allSucceeded = true;
        IEnumerable<string> lines;

        if (consoleArgs.args.Length < 2)
        {
            await Console.Error.WriteLineAsync("Usage: batch <filepath|->");
            return false;
        }

        var source = consoleArgs.args[1];
        bool isStdin = source == "-";

        if (isStdin)
        {
            lines = ReadStdinLines();
        }
        else
        {
            lines = await ReadFileLines(source);
            if (!lines.Any())
            {
                return false;
            }
        }

        foreach (var line in lines)
        {
            if (!ExecuteLine(line))
            {
                allSucceeded = false;
                await Console.Error.WriteLineAsync($"Invocation failed: {line.Trim()}");
            }
            Console.WriteLine(Environment.NewLine);
        }

        return allSucceeded;
    }

    private bool ExecuteLine(string line)
    {
        var lineArgs = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return Run(lineArgs);
    }

    private static async Task<IEnumerable<string>> ReadFileLines(string source)
    {
        IEnumerable<string> readLines;
        try
        {
            readLines = await File.ReadAllLinesAsync(source);
        }
        catch (IOException)
        {
            await Console.Error.WriteLineAsync($"The file: {source} does not exist");
            return [];
        }

        return readLines.Where(l => !string.IsNullOrWhiteSpace(l));
    }

    private IEnumerable<string> ReadStdinLines()
    {
        var afterBatchArgs = string.Join(" ", consoleArgs.args.Skip(2));
        return afterBatchArgs.Split(StdinDelimiter, StringSplitOptions.RemoveEmptyEntries);
    }
}
