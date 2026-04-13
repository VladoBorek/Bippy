using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BusinessLayer.CLI.Commands;
using BusinessLayer.CLI.Parser;
using BusinessLayer.Services;
using BusinessLayer.Services.Interfaces;
using PV286_project;
using BusinessLayer.Cli.Commands.Verify;

namespace PV286.Project.Tests
{
    [TestFixture]
    public class VerifyIntegrationTests
    {
        private const string Mnemonic =
            "photo memory captain decline vendor heavy seminar gloom mouse economy awkward tilt";

        private const string SeedHex =
            "f337beabcfff42915cd9a65fb48745dd0bd122718f04789caeed86cacd35c028" +
            "d151f3f1ed100bf01adedbf734270b269632d851f45cbbf4bdd0ba6c0bce94db";

        private static int CountOccurrences(string text, string needle) =>
            string.IsNullOrEmpty(text) ? 0 : text.Split(needle).Length - 1;

        private static string HexToBin(string hex) =>
            string.Concat(
                Convert.FromHexString(hex)
                    .Select(b => Convert.ToString(b, 2).PadLeft(8, '0'))
            );

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
                        services.AddSingleton<IVerifyService, VerifyService>();

                        services.AddSingleton<IArgParser, ArgParser>();
                        services.AddSingleton<ICliParser, VerifyParser>();
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
        public async Task Verify_KnownMnemonic_AndMatchingHexSeed_PrintsOK()
        {
            var (code, stdout, stderr) = await RunAppAsync(
                "verify",
                "--phrase", Mnemonic,
                "--seed", SeedHex,
                "--format", "hex"
            );

            Assert.That(code, Is.EqualTo(0), stderr);
            Assert.That(stdout.Trim(), Is.EqualTo("OK"));
            Assert.That(CountOccurrences(stdout, "OK"), Is.EqualTo(1));
        }

        [Test]
        [Category("Integration")]
        public async Task Verify_KnownMnemonic_AndMatchingBinarySeed_PrintsOK()
        {
            var seedBin = HexToBin(SeedHex);

            var (code, stdout, stderr) = await RunAppAsync(
                "verify",
                "--phrase", Mnemonic,
                "--seed", seedBin,
                "--format", "bin"
            );

            Assert.That(code, Is.EqualTo(0), stderr);
            Assert.That(stdout.Trim(), Is.EqualTo("OK"));
        }

        [Test]
        [Category("Integration")]
        public async Task Verify_KnownMnemonic_AndWrongSeed_PrintsNOK()
        {
            var wrongSeed =
                "0037beabcfff42915cd9a65fb48745dd0bd122718f04789caeed86cacd35c028" +
                "d151f3f1ed100bf01adedbf734270b269632d851f45cbbf4bdd0ba6c0bce94db";

            var (code, stdout, stderr) = await RunAppAsync(
                "verify",
                "--phrase", Mnemonic,
                "--seed", wrongSeed,
                "--format", "hex"
            );

            Assert.That(code, Is.EqualTo(0), stderr);
            Assert.That(stdout.Trim(), Is.EqualTo("NOK"));
        }

        [Test]
        [Category("Integration")]
        public async Task Verify_InvalidWord_Fails()
        {
            var badMnemonic =
                "photo memory captain decline vendor heavy seminar gloom mouse economy awkward WRONGWORD";

            var (code, stdout, stderr) = await RunAppAsync(
                "verify",
                "--phrase", badMnemonic,
                "--seed", SeedHex,
                "--format", "hex"
            );

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr, Does.Contain("invalid").IgnoreCase);
            Assert.That(stdout + stderr, Does.Contain("mnemonic").IgnoreCase);
        }

        [Test]
        [Category("Integration")]
        public async Task Verify_BadChecksum_Fails()
        {
            var badMnemonic =
                "photo memory captain decline vendor heavy seminar gloom mouse economy awkward atom";

            var (code, stdout, stderr) = await RunAppAsync(
                "verify",
                "--phrase", badMnemonic,
                "--seed", SeedHex,
                "--format", "hex"
            );

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr, Does.Contain("checksum").IgnoreCase);
        }

        [Test]
        [Category("Integration")]
        public async Task Verify_MissingPhrase_Fails()
        {
            var (code, stdout, stderr) = await RunAppAsync(
                "verify",
                "--seed", SeedHex,
                "--format", "hex"
            );

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr, Does.Contain("Option '--phrase' is required").IgnoreCase);
        }

        [Test]
        [Category("Integration")]
        public async Task Verify_MissingSeed_Fails()
        {
            var (code, stdout, stderr) = await RunAppAsync(
                "verify",
                "--phrase", Mnemonic,
                "--format", "hex"
            );

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr, Does.Contain("Option '--seed' is required").IgnoreCase);
        }

        [Test]
        [Category("Integration")]
        public async Task Verify_MissingPhraseValue_Fails()
        {
            var (code, stdout, stderr) = await RunAppAsync(
                "verify",
                "--phrase",
                "--seed", SeedHex,
                "--format", "hex"
            );

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr, Does.Contain("Missing value for '--phrase'.").IgnoreCase);
        }

        [Test]
        [Category("Integration")]
        public async Task Verify_MissingSeedValue_Fails()
        {
            var (code, stdout, stderr) = await RunAppAsync(
                "verify",
                "--phrase", Mnemonic,
                "--seed"
            );

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr, Does.Contain("Missing value for '--seed'.").IgnoreCase);
        }

        [Test]
        [Category("Integration")]
        public async Task Verify_InvalidFormat_Fails()
        {
            var (code, stdout, stderr) = await RunAppAsync(
                "verify",
                "--phrase", Mnemonic,
                "--seed", SeedHex,
                "--format", "HEX"
            );

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr, Does.Contain("Format must be 'hex' or 'bin'."));
        }

        [Test]
        [Category("Integration")]
        public async Task Verify_InvalidHexSeedCharacters_Fails()
        {
            var badSeed = new string('Z', 128);

            var (code, stdout, stderr) = await RunAppAsync(
                "verify",
                "--phrase", Mnemonic,
                "--seed", badSeed,
                "--format", "hex"
            );

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr, Does.Contain("hexadecimal").IgnoreCase);
        }

        [Test]
        [Category("Integration")]
        public async Task Verify_InvalidHexSeedLength_Fails()
        {
            var (code, stdout, stderr) = await RunAppAsync(
                "verify",
                "--phrase", Mnemonic,
                "--seed", "abcd",
                "--format", "hex"
            );

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr, Does.Contain("128 characters").IgnoreCase);
        }

        [Test]
        [Category("Integration")]
        public async Task Verify_InvalidBinarySeedCharacters_Fails()
        {
            var badSeed = new string('2', 512);

            var (code, stdout, stderr) = await RunAppAsync(
                "verify",
                "--phrase", Mnemonic,
                "--seed", badSeed,
                "--format", "bin"
            );

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr, Does.Contain("binary").IgnoreCase);
        }

        [Test]
        [Category("Integration")]
        public async Task Verify_InvalidBinarySeedLength_Fails()
        {
            var shortSeed = new string('0', 511);

            var (code, stdout, stderr) = await RunAppAsync(
                "verify",
                "--phrase", Mnemonic,
                "--seed", shortSeed,
                "--format", "bin"
            );

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr, Does.Contain("512 bits").IgnoreCase);
        }

        [Test]
        [Category("Integration")]
        public async Task Verify_UnknownOption_Fails()
        {
            var (code, stdout, stderr) = await RunAppAsync(
                "verify",
                "--phrase", Mnemonic,
                "--seed", SeedHex,
                "--weird", "x"
            );

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr, Does.Contain("Unrecognized option '--weird'").IgnoreCase);
        }
    }
}
