using BusinessLayer.CLI.Utils.Enums;
using BusinessLayer.Services.Interfaces;
using ResultPattern;
using BusinessLayer.CLI.Utils;

namespace BusinessLayer.CLI.Commands.Decode
{
    public class DecodeParser : ICliParser
    {
        public string CommandName => "decode";
        private readonly IDecodeService decodeService;

        public DecodeParser(IDecodeService decodeService)
        {
            this.decodeService = decodeService;
        }

        public Result<ICliCommand> Parse(string[] args)
        {
            if (args.Length == 0)
                return Result.Fail<ICliCommand>("No mnemonic phrase provided for 'decode'.");

            string? phrase = null;
            ValueFormat format = ValueFormat.Hex; 

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--format":
                        var formatResult = ParseFormat(args, ref i, out format);
                        if (formatResult.IsFailed)
                            return Result.Fail<ICliCommand>(formatResult);
                        break;

                    default:
                        phrase = (phrase == null) ? args[i] : $"{phrase} {args[i]}";
                        break;
                }
            }

            if (phrase == null)
                return Result.Fail<ICliCommand>("No mnemonic phrase provided for 'decode'.");

            return Result.Ok<ICliCommand>(new DecodeCommand(phrase, format, decodeService));
        }

        private static Result ParseFormat(string[] args, ref int i, out ValueFormat format)
        {
            format = ValueFormat.Hex;

            var formatValueResult = ParserUtils.GetRequiredValue(args, ref i, "--format");
            if (formatValueResult.IsFailed)
                return Result.Fail(formatValueResult);

            var parseResult = ParserUtils.ParseFormat(formatValueResult.Value);
            if (parseResult.IsFailed)
                return Result.Fail(parseResult);

            format = parseResult.Value;
            return Result.Ok();
        }
    }
}