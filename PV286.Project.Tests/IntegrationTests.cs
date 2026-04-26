using BusinessLayer.Cli.Commands;
using BusinessLayer.Cli.Commands.Encode;
using BusinessLayer.Cli.Parser;
using BusinessLayer.Services;
using BusinessLayer.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PV286_project;

namespace PV286.Project.Tests
{
    [TestFixture]
    public class AppWorkerIntegrationTests
    {
        private static int CountOccurrences(string text, string needle) =>
            string.IsNullOrEmpty(text) ? 0 : text.Split(needle).Length - 1;

        private async Task<(int exitCode, string stdout, string stderr)> RunAppAsync(params string[] args)
        {
            // capture console
            var origOut = Console.Out;
            var origErr = Console.Error;
            using var outWriter = new StringWriter();
            using var errWriter = new StringWriter();
            Console.SetOut(outWriter);
            Console.SetError(errWriter);

            try
            {
                using var host = Host.CreateDefaultBuilder()
                    .ConfigureLogging(b => b.ClearProviders()) // silence logs
                    .ConfigureServices(services =>
                    {
                        // Pass CLI args into the app via ConsoleArgs (init-only property)
                        services.AddSingleton(_ => new ConsoleArgs { args = args });

                        // Required services
                        services.AddSingleton<IEncodeService, EncodeService>();

                        services.AddSingleton<IArgParser, ArgParser>();

                        // Register command parsers
                        services.AddSingleton<CmdParser, EncodeParser>();

                        // Hosted entry point
                        services.AddHostedService<AppWorker>();
                    })
                    .Build();

                await host.RunAsync(); // AppWorker sets Environment.ExitCode

                return (Environment.ExitCode, outWriter.ToString(), errWriter.ToString());
            }
            finally
            {
                Console.SetOut(origOut);
                Console.SetError(origErr);
            }
        }

        // -------------------------------------------------------------
        // HELP TEST
        // -------------------------------------------------------------
        [Test]
        [Category("Integration")]
        public async Task Help_PrintsUsage_ExitCodeZero()
        {
            var (code, stdout, stderr) = await RunAppAsync("--help");

            Assert.That(code, Is.EqualTo(0), stderr);
            Assert.That(stdout + stderr, Does.Contain("Usage").IgnoreCase);

            // Help should not produce mnemonic/seed blocks
            Assert.That(CountOccurrences(stdout, "Mnemonic : "), Is.EqualTo(0));
            Assert.That(CountOccurrences(stdout, "Seed     : "), Is.EqualTo(0));
        }

        // -------------------------------------------------------------
        // ENCODE COMMAND TESTS
        // -------------------------------------------------------------
        [Test]
        [Category("Integration")]
        public async Task Encode_WithHexEntropy_ProducesMnemonicAndSeed()
        {
            const string hex32 = "00112233445566778899AABBCCDDEEFF"; // 32 hex chars

            var (code, stdout, stderr) =
                await RunAppAsync("encode", "--entropy", hex32, "--format", "hex");

            Assert.That(code, Is.EqualTo(0), stderr);

            Assert.That(CountOccurrences(stdout, "Mnemonic : "), Is.EqualTo(1));
            Assert.That(CountOccurrences(stdout, "Seed     : "), Is.EqualTo(1));
        }

        [Test]
        [Category("Integration")]
        public async Task UnknownOption_FailsWithClearError()
        {
            var (code, stdout, stderr) =
                await RunAppAsync("encode", "--bad-flag");

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr,
                Does.Contain("Unrecognized option '--bad-flag'").IgnoreCase);

            Assert.That(CountOccurrences(stdout, "Mnemonic : "), Is.EqualTo(0));
            Assert.That(CountOccurrences(stdout, "Seed     : "), Is.EqualTo(0));
        }

        [Test]
        [Category("Integration")]
        public async Task Encode_NoEntropy_GeneratesMnemonicAndSeed()
        {
            var (code, stdout, stderr) = await RunAppAsync("encode", "--format", "hex");

            Assert.That(code, Is.EqualTo(0), stderr);
            Assert.That(CountOccurrences(stdout, "Mnemonic : "), Is.EqualTo(1));
            Assert.That(CountOccurrences(stdout, "Seed     : "), Is.EqualTo(1));
        }

        [Test]
        [Category("Integration")]
        public async Task Encode_InvalidHexEntropy_FailsWithError()
        {
            var (code, stdout, stderr) =
                await RunAppAsync("encode", "--entropy", "ZZZZ", "--format", "hex");

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr, Does.Contain("hex").IgnoreCase);

            Assert.That(CountOccurrences(stdout, "Mnemonic : "), Is.EqualTo(0));
            Assert.That(CountOccurrences(stdout, "Seed     : "), Is.EqualTo(0));
        }

        [Test]
        [Category("Integration")]
        public async Task Encode_EntropyWithoutFormat_Fails()
        {
            const string hex32 = "00112233445566778899AABBCCDDEEFF";

            var (code, stdout, stderr) =
                await RunAppAsync("encode", "--entropy", hex32);

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr, Does.Contain("format").IgnoreCase);

            Assert.That(CountOccurrences(stdout, "Mnemonic : "), Is.EqualTo(0));
            Assert.That(CountOccurrences(stdout, "Seed     : "), Is.EqualTo(0));
        }

        // -------------------------------------------------------------
        // BATCH TESTS
        // -------------------------------------------------------------
        [Test]
        [Category("Integration")]
        public async Task Batch_Inline_AllValidCommands_Succeeds()
        {
            var (code, stdout, stderr) = await RunAppAsync(
                "batch", "-",
                "encode --format hex | encode --format bin"
            );

            Assert.That(code, Is.EqualTo(0), stderr);

            int mnemonicCount = CountOccurrences(stdout, "Mnemonic : ");
            int seedCount = CountOccurrences(stdout, "Seed     : ");

            Assert.That(mnemonicCount, Is.EqualTo(2));
            Assert.That(seedCount, Is.EqualTo(2));
        }

        [Test]
        [Category("Integration")]
        public async Task Batch_Inline_OneInvalidCommand_FailsButProcessesOthers()
        {
            var (code, stdout, stderr) = await RunAppAsync(
                "batch", "-",
                "encode --format hex | encode --bad-flag | encode --format bin"
            );

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr, Does.Contain("Invocation failed").IgnoreCase);

            int mnemonicCount = CountOccurrences(stdout, "Mnemonic : ");
            int seedCount = CountOccurrences(stdout, "Seed     : ");
            Assert.That(mnemonicCount, Is.EqualTo(2));
            Assert.That(seedCount, Is.EqualTo(2));
        }

        [Test]
        [Category("Integration")]
        public async Task Batch_File_TwoValidLines_Succeeds()
        {
            var path = Path.GetTempFileName();
            try
            {
                File.WriteAllLines(path, new[]
                {
                    "encode --format hex",
                    "encode --format bin",
                });

                var (code, stdout, stderr) = await RunAppAsync("batch", path);

                Assert.That(code, Is.EqualTo(0), stderr);

                int mnemonicCount = CountOccurrences(stdout, "Mnemonic : ");
                int seedCount = CountOccurrences(stdout, "Seed     : ");
                Assert.That(mnemonicCount, Is.EqualTo(2));
                Assert.That(seedCount, Is.EqualTo(2));
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        [Category("Integration")]
        public async Task Batch_File_InvalidLine_FailsAndPrintsLine()
        {
            var path = Path.GetTempFileName();
            try
            {
                File.WriteAllLines(path, new[]
                {
                    "encode --format hex",
                    "encode --bad-flag",
                    "encode --format bin"
                });

                var (code, stdout, stderr) = await RunAppAsync("batch", path);

                Assert.That(code, Is.Not.EqualTo(0));
                Assert.That(stdout + stderr, Does.Contain("Invocation failed").IgnoreCase);
                Assert.That(stdout + stderr, Does.Contain("--bad-flag"));

                int mnemonicCount = CountOccurrences(stdout, "Mnemonic : ");
                int seedCount = CountOccurrences(stdout, "Seed     : ");
                Assert.That(mnemonicCount, Is.EqualTo(2));
                Assert.That(seedCount, Is.EqualTo(2));
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        [Category("Integration")]
        public async Task Batch_FileNotFound_PrintsError()
        {
            string missing = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.txt");

            var (code, stdout, stderr) = await RunAppAsync("batch", missing);

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr, Does.Contain("does not exist").IgnoreCase);

            Assert.That(CountOccurrences(stdout, "Mnemonic : "), Is.EqualTo(0));
            Assert.That(CountOccurrences(stdout, "Seed     : "), Is.EqualTo(0));
        }

        [Test]
        [Category("Integration")]
        public async Task Batch_MissingSource_PrintsUsage()
        {
            var (code, stdout, stderr) = await RunAppAsync("batch");

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr, Does.Contain("Usage: batch").IgnoreCase);

            Assert.That(CountOccurrences(stdout, "Mnemonic : "), Is.EqualTo(0));
            Assert.That(CountOccurrences(stdout, "Seed     : "), Is.EqualTo(0));
        }

        [Test]
        [Category("Integration")]
        public async Task Batch_Inline_ManyCommands_MixedSuccessAndFailure()
        {
            // A very large inline batch with mixed valid + invalid commands
            string batch =
                "encode --format hex | " + // valid
                "encode --format bin | " + // valid
                "encode | " + // valid (random entropy)
                "encode --entropy ZZZZ --format hex | " + // invalid entropy
                "encode --entropy 0011 --format hex | " + // invalid length (non-multiple-of-8)
                "encode --entropy 00112233445566778899AABBCCDDEEFF --format hex | " + // valid
                "encode --format unknown | " + // invalid format
                "encode --bad-flag | " + // invalid option
                "encode --format bin | " + // valid
                "encode --format hex | " + // valid
                "encode --entropy 00000000000000000000000000000000 --format hex"; // valid

            var (code, stdout, stderr) = await RunAppAsync(
                "batch", "-", batch
            );

            // At least one command must fail -> overall non-zero exit
            Assert.That(code, Is.Not.EqualTo(0));

            // Should contain multiple successes and failures
            Assert.That(stdout, Does.Contain("Mnemonic : "));
            Assert.That(stdout + stderr, Does.Contain("Invocation failed").IgnoreCase);

            // Expected successes: 7 (1,2,3,6,9,10,11)
            int mnemonicCount = CountOccurrences(stdout, "Mnemonic : ");
            int seedCount = CountOccurrences(stdout, "Seed     : ");
            Assert.That(mnemonicCount, Is.GreaterThanOrEqualTo(7));
            Assert.That(seedCount, Is.GreaterThanOrEqualTo(7));

            // Confirm batch continued after failures (blank line prints between invocations)
            Assert.That(stdout, Does.Contain(Environment.NewLine + Environment.NewLine));
        }

        [Test]
        [Category("Integration")]
        public async Task Batch_WithEmojis_WorksAndFailsGracefully()
        {
            // inline batch containing emojis, valid & invalid commands
            string batch =
                "encode --format hex 😀 | " +                       // valid command + emoji
                "encode --format bin 😂😂 | " +                     // valid command + emojis
                "encode --entropy ZZZZ --format hex 😭 | " +        // invalid entropy + emoji
                "encode --bad-flag 🤯 | " +                         // invalid flag + emoji
                "encode --format hex 😎";                           // final valid command + emoji

            var (code, stdout, stderr) = await RunAppAsync(
                "batch", "-",
                batch
            );

            Assert.That(code, Is.Not.EqualTo(0));

            int mnemonicCount = CountOccurrences(stdout, "Mnemonic : ");
            int seedCount = CountOccurrences(stdout, "Seed     : ");
            Assert.That(mnemonicCount, Is.EqualTo(0));
            Assert.That(seedCount, Is.EqualTo(0));

            Assert.That(stdout + stderr, Does.Contain("Invocation failed").IgnoreCase);

            Assert.That(stdout + stderr, Does.Contain("Unrecognized option '--bad-flag'").IgnoreCase);
            Assert.That(stdout + stderr, Does.Contain("hex").IgnoreCase);
        }
    }
}
