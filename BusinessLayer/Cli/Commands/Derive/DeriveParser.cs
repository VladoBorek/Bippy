using BusinessLayer.Cli.Utils;
using BusinessLayer.Cli.Utils.Enums;
using BusinessLayer.Services.Interfaces;
using ResultPattern;

namespace BusinessLayer.Cli.Commands.Derive
{
    public class DeriveParser : ICliParser
    {
        public string CommandName => "derive";

        private readonly IDeriveService deriveService;

        public DeriveParser(IDeriveService deriveService)
        {
            this.deriveService = deriveService;
        }

        public Result<ICliCommand> Parse(string[] args)
        {
            string? entropy = null;
            string? seed = null;
            string? path = null;

            ValueFormat format = ValueFormat.Hex;

            for (int i = 0; i < args.Length; i++)
            {
                var result = args[i] switch
                {
                    "--entropy" => ParseValue(args, ref i, out entropy),
                    "--seed"   => ParseValue(args, ref i, out seed),
                    "--path"   => ParseValue(args, ref i, out path),
                    "--format" => ParseFormat(args, ref i, out format),
                    _ => Result.Fail($"Unrecognized option '{args[i]}' for 'derive'.")
                };

                if (result.IsFailed)
                    return Result.Fail<ICliCommand>(result);
            }

            return Result.Ok<ICliCommand>(
                new DeriveCommand(
                    entropy,
                    seed,
                    path,
                    format,
                    deriveService
                )
            );
        }

        private static Result ParseValue(string[] args, ref int i, out string? value)
        {
            value = null;

            var valueResult = ParserUtils.GetRequiredValue(args, ref i, args[i]);
            if (valueResult.IsFailed)
                return Result.Fail(valueResult);

            value = valueResult.Value;
            return Result.Ok();
        }

        private static Result ParseFormat(
            string[] args,
            ref int i,
            out ValueFormat format)
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