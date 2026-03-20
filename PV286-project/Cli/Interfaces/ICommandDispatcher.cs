using FluentResults;
using PV286_project.Cli.Commands;
namespace PV286_project.Cli.Interfaces
{
    public interface ICommandDispatcher
    {
        Result<string> Dispatch(ParsedCommand command);
    }
}
