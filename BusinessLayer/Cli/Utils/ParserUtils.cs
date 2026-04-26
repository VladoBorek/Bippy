using BusinessLayer.Cli.Utils.Enums;
using ResultPattern;

namespace BusinessLayer.Cli.Utils
{
    public static class ParserUtils
    {
        public static Result<string> GetRequiredValue(string[] args, ref int index, string option)
        {
            if (index + 1 >= args.Length || args[index + 1].StartsWith("--"))
            {
                return Result.Fail<string>($"Missing value for '{option}'.");
            }

            index++;
            return Result.Ok(args[index]);
        }

        public static Result<ValueFormat> ParseFormat(string value) =>
            value switch
            {
                "hex" => Result.Ok(ValueFormat.Hex),
                "bin" => Result.Ok(ValueFormat.Bin),
                _ => Result.Fail<ValueFormat>("Format must be 'hex' or 'bin'."),
            };

        public static byte[] ParseBytes(string value, ValueFormat format) =>
            format == ValueFormat.Hex
                ? Convert.FromHexString(value)
                : Enumerable.Range(0, value.Length / 8)
                            .Select(i => Convert.ToByte(value.Substring(i * 8, 8), 2))
                            .ToArray();
    }
}
