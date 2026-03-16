using FluentResults;
using PV286_project.Cli.Commands;
using PV286_project.Cli.Handlers;
using PV286_project.Cli.Interfaces;

namespace PV286_project.Cli
{
    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly EncodeCommandHandler encodeHandler;
        private readonly HelpCommandHandler helpHandler;

        public CommandDispatcher(EncodeCommandHandler encodeHandler, HelpCommandHandler helpHandler)
        {
            this.encodeHandler = encodeHandler;
            this.helpHandler = helpHandler;

        }

        // Takes a parsed command and forwards it to the appropriate handler
        public Result<string> Dispatch(ParsedCommand command)
        {
            return command switch
            {
                EncodeCommandParsed encode => encodeHandler.Handle(encode),
                HelpCommandParsed help => helpHandler.Handle(help),
                _ => Result.Fail("Unsupported command.")
            };
        }
    }
}
