using ResultPattern;

namespace BusinessLayer.Cli.Utils.Parser
{
    public class FlagParser
    {
        private readonly string _commandName;
        private readonly List<FlagSpec> _specs = new();

        public FlagParser(string commandName) => _commandName = commandName;

        public FlagParser Add(string flag, bool required = false, string? defaultValue = null)
        {
            _specs.Add(new FlagSpec { Flag = flag, Required = required, Default = defaultValue });
            return this;
        }

        public Result<ParsedArgs> Parse(string[] args)
        {
            var known = _specs.ToDictionary(s => s.Flag);
            var values = _specs
                .Where(s => s.Default is not null)
                .ToDictionary(s => s.Flag, s => s.Default!);
            var seen = new HashSet<string>();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] is null)
                    return Result.Fail<ParsedArgs>($"Argument at position {i} cannot be null.");

                var flag = args[i];

                if (!known.ContainsKey(flag))
                    return Result.Fail<ParsedArgs>($"Unrecognized option '{flag}' for '{_commandName}'.");

                if (!seen.Add(flag))
                    return Result.Fail<ParsedArgs>($"Duplicate flag '{flag}' for '{_commandName}'.");

                var valueResult = ParserUtils.GetRequiredValue(args, ref i, flag);
                if (valueResult.IsFailed)
                    return Result.Fail<ParsedArgs>(valueResult);

                values[flag] = valueResult.Value;
            }

            var missing = _specs
                .Where(s => s.Required && !values.ContainsKey(s.Flag))
                .Select(s => s.Flag)
                .ToList();

            if (missing.Count > 0)
                return Result.Fail<ParsedArgs>(
                    $"Missing required option(s) for '{_commandName}': {string.Join(", ", missing)}.");

            return Result.Ok(new ParsedArgs(values));
        }
    }
}
