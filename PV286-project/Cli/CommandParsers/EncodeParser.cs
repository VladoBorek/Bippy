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
                        entropy = CliParser.RequireValue(args, ref i, "--entropy");
                        break;
                    case "--format":
                        format = CliParser.ParseFormat(CliParser.RequireValue(args, ref i, "--format"));
                        formatProvided = true;
                        break;
                    case "--input":  // #TODO: IMPLEENT batch parsing
                        input = CliParser.RequireValue(args, ref i, "--input");
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


