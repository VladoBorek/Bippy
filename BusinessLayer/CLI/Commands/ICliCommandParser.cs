using FluentResults;

namespace BusinessLayer.CLI.Commands
{
    public interface ICliCommandParser
    {
        string CommandName { get; }
        Result<ICliCommand> Parse(string[] args);
    }
}
