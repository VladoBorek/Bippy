using BusinessLayer.CLI.Utils;
using BusinessLayer.CLI.Utils.Enums;
using BusinessLayer.CLI.Validators;
using BusinessLayer.Services.Interfaces;
using ResultPattern;

namespace BusinessLayer.CLI.Commands.Encode
{
    public class EncodeParser : ICliParser
    {
        public string CommandName => "encode";
        private readonly ICommandService commandService;

        public EncodeParser(ICommandService commandService)
        {
            this.commandService = commandService;
        }

        public Result<ICliCommand> Parse(string[] args)
        {
            string? entropy = null;
            ValueFormat format = ValueFormat.Hex;
            bool formatProvided = false;
            byte[]? entropyBytes = null;

            for (int i = 0; i < args.Length; i++)
            {
                var result = args[i] switch
                {
                    "--entropy" => ParseEntropy(args, ref i, out entropy),
                    "--format" => ParseFormat(args, ref i, out format, out formatProvided),
                    _ => Result.Fail($"Unrecognized option '{args[i]}' for 'encode'.")
                };

                if (result.IsFailed)
                    return Result.Fail<ICliCommand>(result);
            }

            var encodeValidationResult = EncodeValidator.IsValidEncode(
                entropy,
                formatProvided,
                format
            );
            if (encodeValidationResult.IsFailed)
            {
                return Result.Fail<ICliCommand>(encodeValidationResult);
            }

            entropyBytes = StringEntropyToBytes(entropy, format);

            return Result.Ok<ICliCommand>(new EncodeCommand(entropyBytes, format, commandService));
        }

        private byte[]? StringEntropyToBytes(string? entropy, ValueFormat format)
        {
            if (entropy is null)
                return null;

            return format == ValueFormat.Hex
                ? Convert.FromHexString(entropy)
                : Enumerable
                    .Range(0, entropy.Length / 8)
                    .Select(i => Convert.ToByte(entropy.Substring(i * 8, 8), 2))
                    .ToArray();
        }

        private static Result ParseEntropy(string[] args, ref int i, out string? entropy)
        {
            entropy = null;

            var entropyResult = ParserUtils.GetRequiredValue(args, ref i, "--entropy");
            if (entropyResult.IsFailed)
                return Result.Fail(entropyResult);

            entropy = entropyResult.Value;
            return Result.Ok();
        }

        private static Result ParseFormat(string[] args, ref int i, out ValueFormat format, out bool formatProvided)
        {
            format = ValueFormat.Hex;
            formatProvided = false;

            var FormatValueResult = ParserUtils.GetRequiredValue(args, ref i, "--format");
            if (FormatValueResult.IsFailed)
                return Result.Fail(FormatValueResult);

            var formatResult = ParserUtils.ParseFormat(FormatValueResult.Value);
            if (formatResult.IsFailed)
                return Result.Fail(formatResult);

            format = formatResult.Value;
            formatProvided = true;
            return Result.Ok();
        }

    }
}
