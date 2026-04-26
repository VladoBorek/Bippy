using BusinessLayer.Cli.Commands;
using BusinessLayer.Cli.Commands.Decode;
using BusinessLayer.Cli.Commands.Derive;
using BusinessLayer.Cli.Commands.Encode;
using BusinessLayer.Cli.Commands.Help;
using BusinessLayer.Cli.Commands.Verify;
using BusinessLayer.Cli.Parser;
using BusinessLayer.Services;
using SharpFuzz;

namespace PV286.Project.Fuzz;

internal static class Program
{
    private const int MaxFuzzerInputLength = 4096;
    private const int MaxRawCommandTokens = 32;

    private const string ValidMnemonic =
        "abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon about";

    private const string ValidSeedHex =
        "00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000";

    private static readonly IArgParser CliParser = CreateCliParser();

    public static void Main()
    {
        Fuzzer.OutOfProcess.Run(ExerciseCliWithFuzzedInput);
    }

    private static ArgParser CreateCliParser()
    {
        return new ArgParser(new ICliParser[]
        {
            new EncodeParser(new EncodeService()),
            new DecodeParser(new DecodeService()),
            new VerifyParser(new VerifyService()),
            new DeriveParser(new DeriveService()),
            new HelpParser()
        });
    }

    private static void ExerciseCliWithFuzzedInput(string rawFuzzerInput)
    {
        var sanitizedInput = SanitizeFuzzerInput(rawFuzzerInput);

        foreach (var commandLineArgs in CreateCommandLineInputs(sanitizedInput))
        {
            TryParseAndRunCommand(commandLineArgs);
        }
    }

    private static string SanitizeFuzzerInput(string input)
    {
        var withoutNullBytes = input.Replace('\0', ' ');
        return LimitLength(withoutNullBytes, MaxFuzzerInputLength);
    }

    private static IReadOnlyList<string[]> CreateCommandLineInputs(string fuzzedInput)
    {
        var firstToken = GetFirstToken(fuzzedInput);
        var format = ChooseFormat(fuzzedInput);
        var phrase = ChoosePhrase(fuzzedInput);
        var derivationPath = ChooseDerivationPath(fuzzedInput);

        var rawCommand = SplitIntoCommandLineArgs(fuzzedInput);

        var encodeCommand = new[]
        {
            "encode", "--entropy", firstToken, "--format", format
        };

        var decodeCommand = new[]
        {
            "decode", phrase, "--format", format
        };

        var verifyCommand = new[]
        {
            "verify", "--phrase", phrase, "--seed", firstToken, "--format", format
        };

        var deriveWithFuzzedInputCommand = new[]
        {
            "derive", "--seed", firstToken, "--path", derivationPath, "--format", format
        };

        var deriveWithValidSeedCommand = new[]
        {
            "derive", "--seed", ValidSeedHex, "--path", derivationPath, "--format", "hex"
        };

        return new[]
        {
            rawCommand,
            encodeCommand,
            decodeCommand,
            verifyCommand,
            deriveWithFuzzedInputCommand,
            deriveWithValidSeedCommand
        };
    }

    private static void TryParseAndRunCommand(string[] commandLineArgs)
    {
        var parsedCommand = CliParser.Parse(commandLineArgs);

        if (parsedCommand.IsFailed)
        {
            return;
        }

        RunCommandWithoutConsoleNoise(parsedCommand.Value);
    }

    private static string ChooseFormat(string input)
    {
        return input.Length % 2 == 0 ? "hex" : "bin";
    }

    private static string ChoosePhrase(string input)
    {
        return string.IsNullOrWhiteSpace(input)
            ? ValidMnemonic
            : input.Trim();
    }

    private static string ChooseDerivationPath(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return "m";
        }

        var firstToken = GetFirstToken(input);

        return firstToken.Length % 2 == 0
            ? "m"
            : firstToken;
    }

    private static string GetFirstToken(string input)
    {
        return SplitIntoCommandLineArgs(input).FirstOrDefault() ?? string.Empty;
    }

    private static string[] SplitIntoCommandLineArgs(string input)
    {
        return input
            .Split(
                new[] { ' ', '\t', '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            )
            .Take(MaxRawCommandTokens)
            .ToArray();
    }

    private static string LimitLength(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static void RunCommandWithoutConsoleNoise(ICliCommand command)
    {
        var originalOut = Console.Out;
        var originalErr = Console.Error;

        try
        {
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);

            command.Handle();
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
        }
    }
}
