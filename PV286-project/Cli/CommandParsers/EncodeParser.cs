using BusinessLayer.enums;
using FluentResults;
using PV286_project.Cli.Commands;
using PV286_project.Cli.Interfaces;

namespace PV286_project.Cli.CommandParsers
{
    // # TODO: CHANGE THIS TO ALSO TAKE INTO ACCOUNT THE POSITIONG OF THE ARGUMENTS
    public class EncodeParser : ICommandParser
    {
        public Result<ParsedCommand> Parse(string[] args)
        {
            string? entropy = null;
            ValueFormat format = ValueFormat.Hex;
            string? input = null;
            bool formatProvided = false;

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
                        var valueResult = CliParser.GetRequiredValue(args, ref i, "--format");
                        if (valueResult.IsFailed)
                            return Result.Fail(valueResult.Errors);

                        var formatResult = CliParser.ParseFormat(valueResult.Value);
                        if (formatResult.IsFailed)
                        {
                            return Result.Fail(formatResult.Errors);
                        }
                        format = formatResult.Value;
                        formatProvided = true;
                        break;

                    case "--input":  // #TODO: implement batch input (so far only placeholder)
                        var inputResult = CliParser.GetRequiredValue(args, ref i, "--input");
                        if (inputResult.IsFailed)
                            return Result.Fail(inputResult.Errors);

                        input = inputResult.Value;
                        break;

                    default:
                        return Result.Fail($"Unrecognized option '{args[i]}' for 'encode'.");

                }
            }

            if (entropy is not null && !formatProvided)
            {
                return Result.Fail("Option '--format' is required when '--entropy' is provided.");
            }

            if (entropy is not null && input is not null)
            {
                return Result.Fail("Use either '--entropy' or '--input', not both.");
            }

            var parsedCommand = new EncodeCommandParsed
            {
                Entropy = entropy,
                Input = input,
                Format = format,
                FormatProvided = formatProvided
            };

            return Result.Ok<ParsedCommand>(parsedCommand);
        }
    }
}


