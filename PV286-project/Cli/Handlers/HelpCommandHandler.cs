using FluentResults;
using PV286_project.Cli.Commands;
using PV286_project.Cli.Interfaces;

namespace PV286_project.Cli.Handlers
{
    public class HelpCommandHandler : ICommandHandler<HelpCommandParsed>
    {
        public Result<string> Handle(HelpCommandParsed command)
        {
            return Result.Ok("VERY NICE HELP MESSAGE #TODO implement");
        }

    }
}
