using BusinessLayer.CLI.Utils.Enums;
using FluentResults;

namespace BusinessLayer.CLI.Utils
{
    public static class ParserUtils
    {
        public static Result<string> GetRequiredValue(string[] args, ref int index, string option)
        {
            if (index + 1 >= args.Length || args[index + 1].StartsWith("--"))
            {
                return Result.Fail($"Missing value for '{option}'.");
            }

            index++;
            return Result.Ok(args[index]);
        }

        public static Result<ValueFormat> ParseFormat(string value) =>
            value switch
            {
                "hex" => Result.Ok(ValueFormat.Hex),
                "bin" => Result.Ok(ValueFormat.Bin),
                _ => Result.Fail("Format must be 'hex' or 'bin'."),
            };
    }
}
