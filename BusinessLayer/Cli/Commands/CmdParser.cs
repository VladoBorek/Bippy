using BusinessLayer.Cli.Utils.Parser;
using ResultPattern;

namespace BusinessLayer.Cli.Commands
{
    public abstract class CmdParser
    {
        public abstract string CommandName { get; }
        protected abstract FlagParser FlagParser();
        protected abstract Result<ICliCommand> Build(ParsedArgs opts);

        public Result<ICliCommand> Parse(string[] args)
        {
            var result = FlagParser().Parse(args);
            if (result.IsFailed) return Result.Fail<ICliCommand>(result);
            return Build(result.Value);
        }
    }
}
