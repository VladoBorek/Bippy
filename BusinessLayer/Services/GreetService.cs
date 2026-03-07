using BusinessLayer.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLayer.Services;

public class GreeterService : IGreetService
{
    private readonly ILogger<GreeterService> logger;
    private readonly ConsoleArgs ConsoleArgs;

    public GreeterService(ILogger<GreeterService> logger, ConsoleArgs consoleArgs)
    {
        this.logger = logger;
        this.ConsoleArgs = consoleArgs;
    }

    public string Greet(string name)
    {
        logger.LogDebug("Greet called with name: {Name}", name);

        logger.LogInformation(
            "Greet called with args: {Args}",
            string.Join(", ", ConsoleArgs.args)
        );

        var message = $"Hello, {name}!";

        logger.LogInformation("Greeting generated: {Message}", message);

        return message;
    }
}
