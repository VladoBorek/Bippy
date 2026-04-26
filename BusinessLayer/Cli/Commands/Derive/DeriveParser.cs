using BusinessLayer.Cli.Utils;
using BusinessLayer.Cli.Utils.Parser;
using BusinessLayer.Services.Interfaces;
using ResultPattern;

namespace BusinessLayer.Cli.Commands.Derive
{
    public class DeriveParser : CmdParser
    {
        public override string CommandName => "derive";
        private readonly IDeriveService deriveService;

        public DeriveParser(IDeriveService deriveService)
        {
            this.deriveService = deriveService;
        }


        protected override FlagParser FlagParser() => new FlagParser("derive")
            .Add("--entropy")
            .Add("--seed")
            .Add("--path")
            .Add("--format", defaultValue: "hex");

        protected override Result<ICliCommand> Build(ParsedArgs opts)
        {
            var formatResult = opts.GetParsed("--format", ParserUtils.ParseFormat);
            if (formatResult.IsFailed) return Result.Fail<ICliCommand>(formatResult);

            var entropyResult = opts.Get("--entropy");
            var entropy = entropyResult.IsFailed ? null : entropyResult.Value;

            var seedResult = opts.Get("--seed");
            var seed = seedResult.IsFailed ? null : seedResult.Value;

            var pathResult = opts.Get("--path");
            var path = pathResult.IsFailed ? null : pathResult.Value;

            return Result.Ok<ICliCommand>(new DeriveCommand(
                entropy,
                seed,
                path,
                formatResult.Value,
                deriveService
            ));
        }
    }
}