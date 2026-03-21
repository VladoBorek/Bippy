using BusinessLayer.CLI.Commands;
using BusinessLayer.CLI.Commands.Help;
using FluentResults;

namespace BusinessLayer.CLI.Parser
{
    public class ArgParser : IArgParser
    {
        private readonly IEnumerable<ICliCommandParser> parsers;

        public ArgParser(IEnumerable<ICliCommandParser> parsers)
        {
            this.parsers = parsers;
        }

        public Result<ICliCommand> Parse(string[] args)
        {
            if (args.Length == 0 || args[0] == "--help")
                return Result.Ok<ICliCommand>(new HelpCommand());

            var parser = parsers.FirstOrDefault(p => p.CommandName == args[0]);
            if (parser is null)
                return Result.Fail($"Unrecognized command '{args[0]}'.");

            return parser.Parse(args.Skip(1).ToArray());
        }
    }
}
