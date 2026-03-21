using BusinessLayer.CLI.Commands;
using FluentResults;

namespace BusinessLayer.CLI.Parser
{
    public interface IArgParser
    {
        public Result<ICliCommand> Parse(string[] args);
    }
}
