internal sealed record HarnessOptions(
    string CorpusDir,
    string FailuresDir,
    bool Smoke,
    bool IncludeDerive,
    bool IncludeDeriveEntropy)
{
    public static HarnessOptions Parse(string[] args)
    {
        var corpus = "PV286.Project.DiffFuzz.Afl/Testcases";
        var failures = "diff-fuzz-findings";
        var smoke = false;
        var includeDerive = false;
        var includeDeriveEntropy = false;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--corpus": corpus = ReadValue(args, ref i); break;
                case "--failures": failures = ReadValue(args, ref i); break;
                case "--smoke": smoke = true; break;
                case "--include-derive": includeDerive = true; break;
                case "--include-derive-entropy": includeDeriveEntropy = true; break;

                // Accepted for backward compatibility with the old process-based script.
                case "--ours": _ = ReadValue(args, ref i); break;
                case "--target": _ = ReadValue(args, ref i); break;
                case "--timeout-ms": _ = ReadValue(args, ref i); break;

                default: throw new ArgumentException($"Unknown argument: {args[i]}");
            }
        }

        return new HarnessOptions(
            Path.GetFullPath(corpus),
            Path.GetFullPath(failures),
            smoke,
            includeDerive,
            includeDeriveEntropy);
    }

    private static string ReadValue(string[] args, ref int index)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"Missing value after {args[index]}");
        }

        return args[++index];
    }
}
