using BusinessLayer.CLI.Commands;
using BusinessLayer.CLI.Commands.Help;
using FluentResults;

namespace BusinessLayer.CLI.Parser
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
                return Result.Ok<ICliCommand>(new HelpCommand());

            var command = commandParsers.FirstOrDefault(p => p.CommandName == args[0]);
            if (command is null)
                return Result.Fail($"Unrecognized command '{args[0]}'.");

            return command.Parse(args.Skip(1).ToArray());
        }
    }
}
