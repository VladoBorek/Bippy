using PV286_project;
using BusinessLayer.Cli.Parser;
using BusinessLayer.Services;
using BusinessLayer.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BusinessLayer.Cli.Commands;
using BusinessLayer.Cli.Commands.Derive;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PV286.Project.Tests
{
    [TestFixture]
    public class DeriveIntegrationTests
    {
        private const string SeedHex = "000102030405060708090a0b0c0d0e0f";

        //  Extract only the real output (ignore host lifecycle logs)
        private static string ExtractKeys(string stdout) =>
            string.Join(
                "\n",
                stdout
                    .Split('\n')
                    .Where(l =>
                        l.TrimStart().StartsWith("XPrv") ||
                        l.TrimStart().StartsWith("XPub"))
            ).Trim();

        private async Task<(int exitCode, string stdout, string stderr)> RunAppAsync(params string[] args)
        {
            // Prevent exit code leaking between tests
            Environment.ExitCode = 0;

            var origOut = Console.Out;
            var origErr = Console.Error;

            using var outWriter = new StringWriter();
            using var errWriter = new StringWriter();

            Console.SetOut(outWriter);
            Console.SetError(errWriter);

            try
            {
                using var host = Host.CreateDefaultBuilder()
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton(_ => new ConsoleArgs { args = args });
                        services.AddSingleton<IDeriveService, DeriveService>();

                        services.AddSingleton<IArgParser, ArgParser>();
                        services.AddSingleton<ICliParser, DeriveParser>();
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
        public async Task Derive_MasterKey_FromSeed_PrintsXprvAndXpub()
        {
            var (code, stdout, stderr) = await RunAppAsync(
                "derive",
                "--seed", SeedHex
            );

            Assert.That(code, Is.EqualTo(0), stderr);

            var keys = ExtractKeys(stdout);
            Assert.That(keys, Does.Contain("XPrv"));
            Assert.That(keys, Does.Contain("XPub"));
        }

        [Test]
        [Category("Integration")]
        public async Task Derive_WithExplicitMasterPath_SameOutput()
        {
            var (_, stdout1, _) = await RunAppAsync(
                "derive",
                "--seed", SeedHex
            );

            var (_, stdout2, _) = await RunAppAsync(
                "derive",
                "--seed", SeedHex,
                "--path", "m"
            );

            Assert.That(
                ExtractKeys(stdout1),
                Is.EqualTo(ExtractKeys(stdout2))
            );
        }

        [Test]
        [Category("Integration")]
        public async Task Derive_NonHardenedChild_Works()
        {
            var (code, stdout, stderr) = await RunAppAsync(
                "derive",
                "--seed", SeedHex,
                "--path", "m/0/1"
            );

            Assert.That(code, Is.EqualTo(0), stderr);

            var keys = ExtractKeys(stdout);
            Assert.That(keys, Does.Contain("XPrv"));
            Assert.That(keys, Does.Contain("XPub"));
        }

        [Test]
        [Category("Integration")]
        public async Task Derive_HardenedChild_Works()
        {
            var (code, stdout, stderr) = await RunAppAsync(
                "derive",
                "--seed", SeedHex,
                "--path", "m/44'/0'/0'"
            );

            Assert.That(code, Is.EqualTo(0), stderr);

            var keys = ExtractKeys(stdout);
            Assert.That(keys, Does.Contain("XPrv"));
            Assert.That(keys, Does.Contain("XPub"));
        }

        [Test]
        [Category("Integration")]
        public async Task Derive_IsDeterministic()
        {
            var (_, stdout1, _) = await RunAppAsync(
                "derive",
                "--seed", SeedHex,
                "--path", "m/0/0"
            );

            var (_, stdout2, _) = await RunAppAsync(
                "derive",
                "--seed", SeedHex,
                "--path", "m/0/0"
            );

            Assert.That(
                ExtractKeys(stdout1),
                Is.EqualTo(ExtractKeys(stdout2))
            );
        }

        [Test]
        [Category("Integration")]
        public async Task Derive_FromEntropy_PrintsXprvAndXpub()
        {
            var entropyHex = "abcdabcdabcdabcdabcdabcdabcdabcd";

            var (code, stdout, stderr) = await RunAppAsync(
                "derive",
                "--entropy", entropyHex
            );

            Assert.That(code, Is.EqualTo(0), stderr);

            var keys = ExtractKeys(stdout);
            Assert.That(keys, Does.Contain("XPrv"));
            Assert.That(keys, Does.Contain("XPub"));
        }

        [Test]
        [Category("Integration")]
        public async Task Derive_BothSeedAndEntropy_Fails()
        {
            var (code, stdout, stderr) = await RunAppAsync(
                "derive",
                "--seed", "abcd",
                "--entropy", "abcd"
            );

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr, Does.Contain("Only one"));
        }

        [Test]
        [Category("Integration")]
        public async Task Derive_MissingSeedAndEntropy_Fails()
        {
            var (code, stdout, stderr) = await RunAppAsync(
                "derive",
                "--path", "m/0"
            );

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr, Does.Contain("Either --entropy or --seed"));
        }

        [Test]
        [Category("Integration")]
        public async Task Derive_InvalidPath_Fails()
        {
            var (code, stdout, stderr) = await RunAppAsync(
                "derive",
                "--seed", SeedHex,
                "--path", "m/x/y"
            );

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr, Does.Contain("Invalid derivation path"));
        }

        [Test]
        [Category("Integration")]
        public async Task Derive_InvalidHex_Fails()
        {
            var badSeed = new string('Z', 10);

            var (code, stdout, stderr) = await RunAppAsync(
                "derive",
                "--seed", badSeed,
                "--format", "hex"
            );

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr, Does.Contain("hex"));
        }

        [Test]
        [Category("Integration")]
        public async Task Derive_UnknownOption_Fails()
        {
            var (code, stdout, stderr) = await RunAppAsync(
                "derive",
                "--seed", SeedHex,
                "--weird", "x"
            );

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(stdout + stderr, Does.Contain("Unrecognized option"));
        }
    }
}