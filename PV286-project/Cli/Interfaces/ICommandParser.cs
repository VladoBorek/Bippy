using FluentResults;
using PV286_project.Cli.Commands;

namespace PV286_project.Cli.Interfaces
{
    internal interface ICommandParser
    {
        Result<ParsedCommand> Parse(string[] args);
    }
}
