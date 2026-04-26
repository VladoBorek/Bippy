using ResultPattern;

namespace BusinessLayer.Cli.Utils.Parser
{
    public class ParsedArgs
    {
        private readonly Dictionary<string, string> _values;

        internal ParsedArgs(Dictionary<string, string> values)
        {
            _values = values;
        }

        private string? GetValue(string flag)
        {
            if (_values.TryGetValue(flag, out var v))
                return v;
            return null;
        }

        public Result<string> Get(string flag)
        {
            var value = GetValue(flag);
            if (value is null)
                return Result.Fail<string>($"Missing value for '{flag}'.");
            return Result.Ok(value);
        }

        public Result<T> GetParsed<T>(string flag, Func<string, Result<T>> parser)
        {
            var raw = GetValue(flag);
            if (raw is null)
                return Result.Fail<T>($"Missing value for '{flag}'.");
            return parser(raw);
        }
    }
}
