using BusinessLayer.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PV286_project.Cli;
using PV286_project.Cli.Commands;
using PV286_project.Cli.Interfaces;
using System.Data;

namespace PV286_project;

// BackgroundService is the standard base class for IHostedService in console apps.
// The host calls StartAsync → ExecuteAsync. When ExecuteAsync returns, the host shuts down.
public class AppWorker : BackgroundService
{
    private readonly ILogger<AppWorker> logger;
    private readonly IHostApplicationLifetime lifetime;
    private readonly ConsoleArgs consoleArgs;
    private readonly CliParser cliParser;
    private readonly ICommandDispatcher commandDispatcher;
    //private readonly IMnemonicService mnemonicService;

    public AppWorker(
      ILogger<AppWorker> logger,
      IHostApplicationLifetime lifetime,
      ConsoleArgs consoleArgs,
      CliParser cliParser,
      ICommandDispatcher commandDispatcher
  //IMnemonicService mnemonicService
  )
    {
        this.logger = logger;
        this.lifetime = lifetime;
        this.consoleArgs = consoleArgs;
        this.cliParser = cliParser;
        this.commandDispatcher = commandDispatcher;
        //this.mnemonicService = mnemonicService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var parsedCommandResult = CliParser.Parse(consoleArgs.args);

            if (parsedCommandResult.IsFailed)
            {
                Console.Error.WriteLine(
                    string.Join(", ", parsedCommandResult.Errors.Select(e => e.Message))
                );
                Environment.ExitCode = 1;
                return Task.CompletedTask;
            }

            ParsedCommand parsedCommand = parsedCommandResult.Value;

            var commandResult = commandDispatcher.Dispatch(parsedCommand);

            if (commandResult.IsFailed)
            {
                Console.Error.WriteLine(
                    string.Join(", ", commandResult.Errors.Select(e => e.Message))
                );
                Environment.ExitCode = 1;
                return Task.CompletedTask;
            }

            Console.WriteLine(commandResult.Value);
            Environment.ExitCode = 0;
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
