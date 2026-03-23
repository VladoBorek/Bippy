using System.Diagnostics;
using System.Text;

namespace PV286.Project.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        private static (int code, string stdout, string stderr) Run(params string[] args)
        {
            // Path to compiled DLL
            var appDll = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..",
                "PV286-project",
                "bin",
                "Release",
                "net9.0",
                "PV286-project.dll"
            ));

            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"\"{appDll}\" {string.Join(" ", args)}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var p = Process.Start(psi)!;
            var stdout = p.StandardOutput.ReadToEnd();
            var stderr = p.StandardError.ReadToEnd();
            p.WaitForExit();

            return (p.ExitCode, stdout, stderr);
        }

        [Test]
        public void Help_PrintsUsage_ExitCodeZero()
        {
            var (code, @out, err) = Run("--help");

            Assert.That(code, Is.EqualTo(0), err);
            Assert.That(@out + err, Does.Contain("Usage").IgnoreCase);
        }

        [Test]
        public void Encode_WithHexEntropy_ProducesMnemonicAndSeed()
        {
            const string hex32 = "00112233445566778899AABBCCDDEEFF"; // 32 chars
            var (code, @out, err) = Run("encode", "--entropy", hex32, "--format", "hex");

            Assert.That(code, Is.EqualTo(0), err);
            Assert.That(@out, Does.Contain("Mnemonic : "));
            Assert.That(@out, Does.Contain("Seed     : "));
        }

        [Test]
        public void UnknownOption_Fails_NonZeroExit_WithClearMessage()
        {
            var (code, @out, err) = Run("encode", "--bad-flag");

            Assert.That(code, Is.Not.EqualTo(0));
            Assert.That(@out + err, Does.Contain("Unrecognized option '--bad-flag'").IgnoreCase);
        }
    }
}
