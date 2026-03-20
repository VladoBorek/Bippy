using BusinessLayer.enums;
using BusinessLayer.Validators;
using FluentResults;
using PV286_project.Cli.Commands;
using PV286_project.Cli.Interfaces;

namespace PV286_project.Cli.CommandParsers
{
    public class EncodeParser : ICommandParser
    {
        public Result<ParsedCommand> Parse(string[] args)
        {
            string? entropy = null;
            ValueFormat format = ValueFormat.Hex;
            bool formatProvided = false;
            byte[]? entropyBytes = null;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--entropy":

                        var entropyResult = CliParser.GetRequiredValue(args, ref i, "--entropy");
                        if (entropyResult.IsFailed)
                        {
                            return Result.Fail(entropyResult.Errors);
                        }

                        entropy = entropyResult.Value;
                        break;

                    case "--format":
                        var FormatValueResult = CliParser.GetRequiredValue(args, ref i, "--format");
                        if (FormatValueResult.IsFailed)
                            return Result.Fail(FormatValueResult.Errors);

                        var formatResult = CliParser.ParseFormat(FormatValueResult.Value);
                        if (formatResult.IsFailed)
                        {
                            return Result.Fail(formatResult.Errors);
                        }
                        format = formatResult.Value;
                        formatProvided = true;
                        break;


                    //case "--batch":  // #TODO: implement batch input (so far only placeholder)
                    // encode --batch encode-batch.txt
                    /*
                     * encode --batch -
                        (stdin:)
                        --entropy 00000000000000000000000000000000 --format hex
                        --entropy 11111111111111111111111111111111 --format hex
                     */

                    default:
                        return Result.Fail($"Unrecognized option '{args[i]}' for 'encode'.");

                }
            }

            var encodeValidationResult = EncodeValidator.IsValidEncode(entropy, formatProvided, format);
            if (encodeValidationResult.IsFailed)
            {
                return Result.Fail(encodeValidationResult.Errors);
            }

            entropyBytes = StringEntropyToBytes(entropy, format);
            return Result.Ok<ParsedCommand>(new EncodeCommandParsed(entropyBytes, format));
        }
        private static byte[]? StringEntropyToBytes(string? entropy, ValueFormat format)
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
    }
}


