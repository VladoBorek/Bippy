using BusinessLayer.Cli.Commands;
using ResultPattern;

namespace BusinessLayer.Cli.Parser
{
    public interface IArgParser
    {
        public Result<ICliCommand> Parse(string[] args);
    }
}
