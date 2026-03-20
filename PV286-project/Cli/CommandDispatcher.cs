using PV286_project.Cli.Commands;
using BusinessLayer.Services;
using BusinessLayer.Services.Interfaces;
using FluentResults;
using PV286_project.Cli.Handlers;
using PV286_project.Cli.Interfaces;

namespace PV286_project.Cli
{
    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly HelpCommandHandler helpHandler;
        private readonly IMnemonicService mnemonicService;

        public CommandDispatcher(IMnemonicService mnemonicService, HelpCommandHandler helpHandler)
        {
            this.mnemonicService = mnemonicService;
            this.helpHandler = helpHandler;
        }

        public Result<string> Dispatch(ParsedCommand command)
        {
            return command switch
            {
                EncodeCommandParsed encode => mnemonicService.Handle(encode.Entropy, encode.Format),
                HelpCommandParsed help => helpHandler.Handle(help),
                _ => Result.Fail("Unsupported command.")
            };
        }
    }
}
