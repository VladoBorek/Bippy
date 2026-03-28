using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BusinessLayer.CLI.Parser;
using BusinessLayer.CLI.Commands;
using BusinessLayer.CLI.Commands.Decode;
using BusinessLayer.Services;
using BusinessLayer.Services.Interfaces;
using PV286_project;

namespace PV286.Project.Tests
{
    [TestFixture]
    public class DecodeIntegrationTests
    {
        private static int CountOccurrences(string text, string needle) =>
            string.IsNullOrEmpty(text) ? 0 : text.Split(needle).Length - 1;

        private async Task<(int exitCode, string stdout, string stderr)> RunAppAsync(params string[] args)
        {
            var origOut = Console.Out;
            var origErr = Console.Error;
            using var outWriter = new StringWriter();
            using var errWriter = new StringWriter();
            Console.SetOut(outWriter);
            Console.SetError(errWriter);

            try
            {
                using var host = Host.CreateDefaultBuilder()
                    .ConfigureLogging(b => b.ClearProviders())
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton(_ => new ConsoleArgs { args = args });
                        services.AddSingleton<ICommandService, CommandService>();
                        services.AddSingleton<IArgParser, ArgParser>();

                        // Register DecodeParser
                        services.AddSingleton<ICliParser, DecodeParser>();

                        services.AddHostedService<AppWorker>();
                    })
                    .Build();

                await host.RunAsync();
                return (Environment.ExitCode, outWriter.ToString(), errWriter.ToString());
            }
            finally
            {
                Console.SetOut(origOut);
                Console.SetError(origErr);
            }
        }

        [Test]
        [Category("Integration")]
        public async Task Decode_KnownMnemonic_HexOutput()
        {
            const string mnemonic =
                "photo memory captain decline vendor heavy seminar gloom mouse economy awkward tilt";

            const string expectedEntropyHex = "A3B15C889C7F22D4F0E31B90A8C44371";
            const string expectedSeedHex =
                "f337beabcfff42915cd9a65fb48745dd0bd122718f04789caeed86cacd35c028" +
                "d151f3f1ed100bf01adedbf734270b269632d851f45cbbf4bdd0ba6c0bce94db";

            var (code, stdout, stderr) =
                await RunAppAsync("decode", mnemonic, "--format", "hex");

            Assert.That(code, Is.EqualTo(0), stderr);
            Assert.That(CountOccurrences(stdout, "Mnemonic : "), Is.EqualTo(1));
            Assert.That(CountOccurrences(stdout, "Entropy  : "), Is.EqualTo(1));
            Assert.That(CountOccurrences(stdout, "Seed     : "), Is.EqualTo(1));

            Assert.That(stdout, Does.Contain(expectedEntropyHex).IgnoreCase);
            Assert.That(stdout, Does.Contain(expectedSeedHex).IgnoreCase);
        }

        [Test]
        [Category("Integration")]
        public async Task Decode_KnownMnemonic_BinOutput()
        {
            const string mnemonic =
                "photo memory captain decline vendor heavy seminar gloom mouse economy awkward tilt";

            const string expectedEntropyBin =
                "10100011101100010101110010001000100111000111111100100010110101001111000011100011000110111001000010101000110001000100001101110001";

            var (code, stdout, stderr) =
                await RunAppAsync("decode", mnemonic, "--format", "bin");

            Assert.That(code, Is.EqualTo(0), stderr);
            Assert.That(CountOccurrences(stdout, "Mnemonic : "), Is.EqualTo(1));
            Assert.That(CountOccurrences(stdout, "Entropy  : "), Is.EqualTo(1));
            Assert.That(CountOccurrences(stdout, "Seed     : "), Is.EqualTo(1));

            Assert.That(stdout, Does.Contain(expectedEntropyBin));
        }

        // -------------------------------------------------------------
        // INVALID INPUT TESTS
        // -------------------------------------------------------------

        [Test]
        [Category("Integration")]
        public async Task Decode_InvalidWord_Fails()
        {
            const string mnemonic =
                "photo memory captain decline vendor heavy seminar gloom mouse economy awkward WRONGWORD";

            var (code, stdout, stderr) =
                await RunAppAsync("decode", mnemonic);

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr, Does.Contain("invalid").IgnoreCase
                .Or.Contain("word").IgnoreCase);

            Assert.That(CountOccurrences(stdout, "Entropy  : "), Is.EqualTo(0));
            Assert.That(CountOccurrences(stdout, "Seed     : "), Is.EqualTo(0));
        }

        [Test]
        [Category("Integration")]
        public async Task Decode_BadChecksum_Fails()
        {
            // Identical to real mnemonic except for last word.
            const string mnemonic =
                "photo memory captain decline vendor heavy seminar gloom mouse economy awkward atom";

            var (code, stdout, stderr) =
                await RunAppAsync("decode", mnemonic);

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr,
                Does.Contain("checksum").IgnoreCase
                    .Or.Contain("invalid").IgnoreCase);

            Assert.That(CountOccurrences(stdout, "Entropy  : "), Is.EqualTo(0));
            Assert.That(CountOccurrences(stdout, "Seed     : "), Is.EqualTo(0));
        }

        [Test]
        [Category("Integration")]
        public async Task Decode_WrongWordCount_Fails()
        {
            // 11 words (must be 12/15/18/21/24)
            const string mnemonic =
                "photo memory captain decline vendor heavy seminar gloom mouse economy awkward";

            var (code, stdout, stderr) =
                await RunAppAsync("decode", mnemonic);

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr,
                Does.Contain("word").IgnoreCase
                    .Or.Contain("length").IgnoreCase);

            Assert.That(CountOccurrences(stdout, "Entropy  : "), Is.EqualTo(0));
            Assert.That(CountOccurrences(stdout, "Seed     : "), Is.EqualTo(0));
        }

        [Test]
        [Category("Integration")]
        public async Task Decode_EmptyString_Fails()
        {
            var (code, stdout, stderr) =
                await RunAppAsync("decode", "");

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr,
                Does.Contain("empty").IgnoreCase
                    .Or.Contain("mnemonic").IgnoreCase
                    .Or.Contain("no mnemonic").IgnoreCase);

            Assert.That(CountOccurrences(stdout, "Entropy  : "), Is.EqualTo(0));
            Assert.That(CountOccurrences(stdout, "Seed     : "), Is.EqualTo(0));
        }

        [Test]
        [Category("Integration")]
        public async Task Decode_NullArgument_Fails()
        {
            var (code, stdout, stderr) =
                await RunAppAsync("decode", (string)null);

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr,
                Does.Contain("mnemonic").IgnoreCase
                    .Or.Contain("null").IgnoreCase);

            Assert.That(CountOccurrences(stdout, "Entropy  : "), Is.EqualTo(0));
            Assert.That(CountOccurrences(stdout, "Seed     : "), Is.EqualTo(0));
        }

        [Test]
        [Category("Integration")]
        public async Task Decode_ExtraGarbageArguments_Fails()
        {
            var (code, stdout, stderr) =
                await RunAppAsync("decode", "photo", "garbage", "stuff");

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr,
                Does.Contain("invalid").IgnoreCase
                    .Or.Contain("unrecognized").IgnoreCase);

            Assert.That(CountOccurrences(stdout, "Entropy  : "), Is.EqualTo(0));
            Assert.That(CountOccurrences(stdout, "Seed     : "), Is.EqualTo(0));
        }
    }
}