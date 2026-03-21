using ResultPattern;

namespace BusinessLayer.CLI.Commands
{
    public interface ICliParser
    {
        string CommandName { get; }
        Result<ICliCommand> Parse(string[] args);
    }
}
