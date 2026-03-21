using BusinessLayer.CLI.Commands;
using ResultPattern;

namespace BusinessLayer.CLI.Parser
{
    public interface IArgParser
    {
        public Result<ICliCommand> Parse(string[] args);
    }
}
