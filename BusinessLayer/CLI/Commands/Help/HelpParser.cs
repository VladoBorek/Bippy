using FluentResults;

namespace BusinessLayer.CLI.Commands.Help
{
    public class HelpParser : ICliCommandParser
    {
        public string CommandName => "help";

        public Result<ICliCommand> Parse(string[] args)
        {
            return Result.Ok<ICliCommand>(new HelpCommand());
        }
    }
}
