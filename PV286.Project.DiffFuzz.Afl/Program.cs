using SharpFuzz;

internal static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            var options = HarnessOptions.Parse(args);
            Directory.CreateDirectory(options.FailuresDir);

            if (options.Smoke)
            {
                var smokeComparer = CreateComparer(options);

                foreach (var file in Directory.EnumerateFiles(options.CorpusDir).OrderBy(x => x))
                {
                    smokeComparer.Compare(File.ReadAllText(file));
                }

                Console.WriteLine("Smoke differential run finished.");
                return 0;
            }

            DifferentialComparer? fuzzComparer = null;

            Fuzzer.OutOfProcess.Run(rawInput =>
            {
                fuzzComparer ??= CreateComparer(options);
                fuzzComparer.Compare(rawInput);
            });

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static DifferentialComparer CreateComparer(HarnessOptions options)
    {
        return new DifferentialComparer(
            options.FailuresDir,
            options.IncludeDerive,
            options.IncludeDeriveEntropy);
    }
}
