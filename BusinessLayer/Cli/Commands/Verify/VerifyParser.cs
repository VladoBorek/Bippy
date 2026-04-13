using BusinessLayer.Cli.Validators;
using BusinessLayer.CLI.Commands;
using BusinessLayer.CLI.Utils;
using BusinessLayer.CLI.Utils.Enums;
using BusinessLayer.CLI.Validators;
using BusinessLayer.Services.Interfaces;
using ResultPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Cli.Commands.Verify
{
    public class VerifyParser : ICliParser
    {
        public string CommandName => "verify";
        private readonly ICommandService commandService;
        public VerifyParser(ICommandService commandService)
        {
            this.commandService = commandService;
        }

        public Result<ICliCommand> Parse(string[] args)
        {
            string? phrase = null;
            string? seed = null;
            ValueFormat format = ValueFormat.Hex;

            for (int i = 0; i < args.Length; i++)
            {
                var result = args[i] switch
                {
                    "--phrase" => ParsePhrase(args, ref i, out phrase),
                    "--seed" => ParseSeed(args, ref i, out seed),
                    "--format" => ParseFormat(args, ref i, out format),
                    _ => Result.Fail($"Unrecognized option '{args[i]}' for 'verify'.")
                };

                if (result.IsFailed)
                    return Result.Fail<ICliCommand>(result);
            }

            if (string.IsNullOrWhiteSpace(phrase))
                return Result.Fail<ICliCommand>("Option '--phrase' is required for 'verify'.");
            if (string.IsNullOrWhiteSpace(seed))
                return Result.Fail<ICliCommand>("Option '--seed' is required for 'verify'.");

            var seedValidationResult = SeedValidator.IsValidSeed(seed, format);
           
            if (seedValidationResult.IsFailed)
                return Result.Fail<ICliCommand>(seedValidationResult);

            byte[] seedBytes = StringSeedToBytes(seed, format);

            return Result.Ok<ICliCommand>(
                new VerifyCommand(phrase, seedBytes, commandService)
            );
        }

        private static Result ParsePhrase(string[] args, ref int i, out string? phrase)
        {
            phrase = null;

            var phraseResult = ParserUtils.GetRequiredValue(args, ref i, "--phrase");
            if (phraseResult.IsFailed)
                return Result.Fail(phraseResult);

            phrase = phraseResult.Value;
            return Result.Ok();
        }

        private static Result ParseSeed(string[] args, ref int i, out string? seed)
        {
            seed = null;

            var seedResult = ParserUtils.GetRequiredValue(args, ref i, "--seed");
            if (seedResult.IsFailed)
                return Result.Fail(seedResult);

            seed = seedResult.Value;
            return Result.Ok();
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


        private static byte[] StringSeedToBytes(string seed, ValueFormat format)
        {
            return format == ValueFormat.Hex
                ? Convert.FromHexString(seed)
                : Enumerable.Range(0, seed.Length / 8)
                    .Select(i => Convert.ToByte(seed.Substring(i * 8, 8), 2))
                    .ToArray();
        }
    }
}
