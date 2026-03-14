using System.Data;
using BusinessLayer.Services;
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
    private readonly ConsoleArgs consoleArgs;
    private readonly IMnemonicService mnemonicService;

    public AppWorker(
        IGreetService greeterService,
        ILogger<AppWorker> logger,
        IHostApplicationLifetime lifetime,
        ConsoleArgs consoleArgs,
        IMnemonicService mnemonicService
    )
    {
        this.greeterService = greeterService;
        this.logger = logger;
        this.lifetime = lifetime;
        this.consoleArgs = consoleArgs;
        this.mnemonicService = mnemonicService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            if (consoleArgs.args.Length == 0)
            {
                CallHelp();
                return Task.CompletedTask;
            }

            switch (consoleArgs.args[0])
            {
                case "mnemonic":
                    CallMnemonicSeed();
                    break;
                default:
                    CallHelp();
                    break;
            }
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

    private void CallMnemonicSeed()
    {
        var idx = Array.IndexOf(consoleArgs.args, "--entropy");
        var entropy = idx >= 0 ? consoleArgs.args.ElementAtOrDefault(idx + 1)?.Trim() : null;

        if (idx != -1 && entropy is null)
        {
            Console.Error.WriteLine("Valid entropy value must be provided");
            return;
        }

        var res = mnemonicService.GetMnemonicSeed(entropy);

        if (res.IsFailed)
        {
            logger.LogError(string.Join(", ", res.Errors.Select(e => e.Message)));
            return;
        }

        var mnemonicSeed = res.Value;

        Console.WriteLine($"Mnemonic : {mnemonicSeed.Mnemonic}");
        Console.WriteLine($"Seed     : {Convert.ToHexString(mnemonicSeed.Seed).ToLower()}");
    }

    private void CallHelp()
    {
        Console.WriteLine("Help tooltip not implemented");
    }
}
