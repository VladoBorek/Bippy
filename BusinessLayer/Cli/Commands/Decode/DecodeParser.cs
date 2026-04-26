using BusinessLayer.Cli.Utils;
using BusinessLayer.Cli.Utils.Parser;
using BusinessLayer.Services.Interfaces;
using ResultPattern;

namespace BusinessLayer.Cli.Commands.Decode
{

    public class DecodeParser : CmdParser
    {
        public override string CommandName => "decode";
        private readonly IDecodeService decodeService;

        public DecodeParser(IDecodeService decodeService)
        {
            this.decodeService = decodeService;
        }


        protected override FlagParser FlagParser() => new FlagParser("decode")
            .Add("--words", required: true)
            .Add("--format", defaultValue: "hex");

        protected override Result<ICliCommand> Build(ParsedArgs opts)
        {
            var wordsResult = opts.Get("--words");
            if (wordsResult.IsFailed) return Result.Fail<ICliCommand>(wordsResult);

            var formatResult = opts.GetParsed("--format", ParserUtils.ParseFormat);
            if (formatResult.IsFailed) return Result.Fail<ICliCommand>(formatResult);

            return Result.Ok<ICliCommand>(new DecodeCommand(wordsResult.Value, formatResult.Value, decodeService));
        }
    }
}