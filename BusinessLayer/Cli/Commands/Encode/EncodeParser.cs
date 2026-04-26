using BusinessLayer.Cli.Utils;
using BusinessLayer.Cli.Utils.Enums;
using BusinessLayer.Cli.Utils.Parser;
using BusinessLayer.Services.Interfaces;
using ResultPattern;

namespace BusinessLayer.Cli.Commands.Encode
{
    public class EncodeParser : CmdParser
    {
        public override string CommandName => "encode";
        private readonly IEncodeService encodeService;

        public EncodeParser(IEncodeService encodeService)
        {
            this.encodeService = encodeService;
        }


        protected override FlagParser FlagParser() => new FlagParser("encode")
            .Add("--entropy")
            .Add("--format");

        protected override Result<ICliCommand> Build(ParsedArgs opts)
        {
            var entropyResult = opts.Get("--entropy");
            var entropy = entropyResult.IsFailed ? null : entropyResult.Value;

            var formatResult = opts.Get("--format");
            var formatProvided = !formatResult.IsFailed;

            ValueFormat format = ValueFormat.Hex;
            if (formatProvided)
            {
                var parsedFormat = ParserUtils.ParseFormat(formatResult.Value);
                if (parsedFormat.IsFailed) return Result.Fail<ICliCommand>(parsedFormat);
                format = parsedFormat.Value;
            }

            var validation = BusinessLayer.Cli.Validators.EncodeValidator.IsValidEncode(entropy, formatProvided, format);
            if (validation.IsFailed) return Result.Fail<ICliCommand>(validation);

            var entropyBytes = entropy == null ? null : ParserUtils.ParseBytes(entropy, format);
            return Result.Ok<ICliCommand>(new EncodeCommand(entropyBytes, format, encodeService));
        }
    }
}
