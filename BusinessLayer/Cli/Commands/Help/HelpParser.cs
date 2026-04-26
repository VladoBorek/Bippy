using BusinessLayer.Cli.Utils.Parser;
using ResultPattern;

namespace BusinessLayer.Cli.Commands.Help
{
    public class HelpParser : CmdParser
    {
        public override string CommandName => "help";

        protected override FlagParser FlagParser() => new FlagParser("help");

        protected override Result<ICliCommand> Build(ParsedArgs opts)
        {
            return Result.Ok<ICliCommand>(new HelpCommand());
        }
    }
}
