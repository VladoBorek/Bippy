using PV286_project.Cli.Commands;
using FluentResults;

namespace PV286_project.Cli.Interfaces
{
    public interface ICommandHandler<TCommand> where TCommand : ParsedCommand
    {
        Result<string> Handle(TCommand command);
    }
}
