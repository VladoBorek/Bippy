using BusinessLayer.enums;
using BusinessLayer.Services;
using FluentResults;
using PV286_project.Cli.CommandParsers;
using PV286_project.Cli.Commands;
using PV286_project.Cli.Interfaces;

namespace PV286_project.Cli
{
    public class CliParser
    {
        private readonly ConsoleArgs consoleArgs;

        public CliParser(ConsoleArgs consoleArgs)
        {
            this.consoleArgs = consoleArgs;
        }

        public static Result<ParsedCommand> Parse(string[] args)
        {
            if (args.Length == 0 || args[0] == "--help")
            {
                return Result.Ok<ParsedCommand>(new HelpCommandParsed()); ;
            }

            var command = args[0];
            var commandArgs = args.Skip(1).ToArray();

            ICommandParser parser = command switch
            {
                "encode" => new EncodeParser(),
                //"decode" => new DecodeParser(),
                //"verify" => new VerifyParser(),
                //"xkey" => new XKeyParser(),
                _ => null!
            };

            if (parser is null)
            {
                return Result.Fail($"Unrecognized command '{command}'.");
            }

            return parser.Parse(commandArgs);

        }

        public static Result<string> GetRequiredValue(string[] args, ref int index, string option)
        {
            if (index + 1 >= args.Length || args[index + 1].StartsWith("--"))
            {
                return Result.Fail($"Missing value for '{option}'.");
            }

            index++;
            return Result.Ok(args[index]);
        }

        public static Result<ValueFormat> ParseFormat(string value) => value switch
        {
            "hex" => Result.Ok(ValueFormat.Hex),
            "bin" => Result.Ok(ValueFormat.Bin),
            _ => Result.Fail("Format must be 'hex' or 'bin'."),
        };

    }
}
