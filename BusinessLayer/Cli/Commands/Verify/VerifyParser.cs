using BusinessLayer.Cli.Utils;
using BusinessLayer.Cli.Utils.Parser;
using BusinessLayer.Cli.Validators;
using BusinessLayer.Services.Interfaces;
using ResultPattern;

namespace BusinessLayer.Cli.Commands.Verify
{
    public class VerifyParser : CmdParser
    {
        public override string CommandName => "verify";
        private readonly IVerifyService verifyService;

        public VerifyParser(IVerifyService verifyService)
        {
            this.verifyService = verifyService;
        }

        protected override FlagParser FlagParser() => new FlagParser("verify")
            .Add("--phrase", required: true)
            .Add("--seed", required: true)
            .Add("--format", defaultValue: "hex");

        protected override Result<ICliCommand> Build(ParsedArgs opts)
        {
            var phraseResult = opts.Get("--phrase");
            if (phraseResult.IsFailed) return Result.Fail<ICliCommand>(phraseResult);

            var seedResult = opts.Get("--seed");
            if (seedResult.IsFailed) return Result.Fail<ICliCommand>(seedResult);

            var formatResult = opts.GetParsed("--format", ParserUtils.ParseFormat);
            if (formatResult.IsFailed) return Result.Fail<ICliCommand>(formatResult);

            var seedValidation = SeedValidator.IsValidSeed(seedResult.Value, formatResult.Value);
            if (seedValidation.IsFailed) return Result.Fail<ICliCommand>(seedValidation);

            var seedBytes = ParserUtils.ParseBytes(seedResult.Value, formatResult.Value);
            return Result.Ok<ICliCommand>(new VerifyCommand(phraseResult.Value, seedBytes, verifyService));
        }
    }
}
