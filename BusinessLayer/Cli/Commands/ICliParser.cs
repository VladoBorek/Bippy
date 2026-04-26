using ResultPattern;

namespace BusinessLayer.Cli.Commands
{
    public interface ICliParser
    {
        string CommandName { get; }
        Result<ICliCommand> Parse(string[] args);
    }
}
