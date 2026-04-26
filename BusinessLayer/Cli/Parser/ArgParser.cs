using BusinessLayer.Cli.Commands;
using BusinessLayer.Cli.Commands.Help;
using ResultPattern;

namespace BusinessLayer.Cli.Parser
{
    public class ArgParser : IArgParser
    {
        private readonly IReadOnlyDictionary<string, CmdParser> handlers;

        public ArgParser(IEnumerable<CmdParser> handlers)
        {
            this.handlers = handlers.ToDictionary(h => h.CommandName);
        }

        public Result<ICliCommand> Parse(string[] args)
        {
            if (args.Length == 0 || args[0] == "--help")
            {
                if (args.Length > 1)
                    return Result.Fail<ICliCommand>("The '--help' flag cannot be used with other arguments.");
                return Result.Ok<ICliCommand>(new HelpCommand());
            }

            if (!handlers.TryGetValue(args[0], out var handler))
                return Result.Fail<ICliCommand>($"Unrecognized command '{args[0]}'.");

            return handler.Parse(args[1..]);
        }
    }
}