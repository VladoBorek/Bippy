using BusinessLayer.Cli.Commands;
using BusinessLayer.Cli.Commands.Help;
using ResultPattern;

namespace BusinessLayer.Cli.Parser
{
    public class ArgParser : IArgParser
    {
        private readonly IEnumerable<ICliParser> commandParsers;

        public ArgParser(IEnumerable<ICliParser> commandParsers)
        {
            this.commandParsers = commandParsers;
        }

        public Result<ICliCommand> Parse(string[] args)
        {
            if (args.Length == 0 || args[0] == "--help")
            {
                if(args.Length > 1) return Result.Fail<ICliCommand>("The '--help' flag cannot be used with other arguments.");
                
                return Result.Ok<ICliCommand>(new HelpCommand());
            }

            var commandParser = commandParsers.FirstOrDefault(p => p.CommandName == args[0]);

            if (commandParser is null)
                return Result.Fail<ICliCommand>($"Unrecognized command '{args[0]}'.");

            return commandParser.Parse(args.Skip(1).ToArray());
        }
    }
}
