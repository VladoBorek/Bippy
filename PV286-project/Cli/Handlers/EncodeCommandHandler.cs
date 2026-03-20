//using BusinessLayer.Services.Interfaces;
//using FluentResults;
//using PV286_project.Cli.Commands;
//using PV286_project.Cli.Interfaces;

//namespace PV286_project.Cli.Handlers
//{
//    public class EncodeCommandHandler : ICommandHandler<EncodeCommandParsed>
//    {
//        private readonly IMnemonicService mnemonicService;

//        public EncodeCommandHandler(IMnemonicService mnemonicService)
//        {
//            this.mnemonicService = mnemonicService;

//        }

//        public Result<string> Handle(EncodeCommandParsed command)
//        {
//            var mnemonicSeedResult = mnemonicService.GetMnemonicSeed(command.Entropy, command.Format);
//            var output = mnemonicSeedResult.Value.ToString();
//            return Result.Ok(output);
//        }
//    }
//}
