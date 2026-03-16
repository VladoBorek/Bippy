using FluentResults;
using PV286_project.Cli.Commands;

namespace PV286_project.Cli.Interfaces
{
    public interface ICommandHandler<TCommand> where TCommand : ParsedCommand
    {
        Result<string> Handle(TCommand command);
    }
}
