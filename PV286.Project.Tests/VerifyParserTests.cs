using BusinessLayer.Cli.Commands.Verify;
using BusinessLayer.Services.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PV286.Project.Tests
{
    [TestFixture]
    public class VerifyParserTests
    {
        private Mock<ICommandService> _commandServiceMock = null!;

        private const string ValidMnemonic =
            "photo memory captain decline vendor heavy seminar gloom mouse economy awkward tilt";

        private const string ValidSeedHex =
            "f337beabcfff42915cd9a65fb48745dd0bd122718f04789caeed86cacd35c028" +
            "d151f3f1ed100bf01adedbf734270b269632d851f45cbbf4bdd0ba6c0bce94db";

        [SetUp]
        public void SetUp()
        {
            _commandServiceMock = new Mock<ICommandService>(MockBehavior.Strict);
        }

        [Test]
        public void Parse_ValidHexPhraseAndSeed_ReturnsVerifyCommand()
        {
            var parser = new VerifyParser(_commandServiceMock.Object);

            var result = parser.Parse(new[]
            {
                "--phrase", ValidMnemonic,
                "--seed", ValidSeedHex,
                "--format", "hex"
            });

            Assert.That(result.IsSuccess, Is.True, result.Error);
            Assert.That(result.Value, Is.InstanceOf<VerifyCommand>());
        }

        [Test]
        public void Parse_ValidBinarySeed_ReturnsVerifyCommand()
        {
            var parser = new VerifyParser(_commandServiceMock.Object);
            var seedBin = new string('0', 512);

            var result = parser.Parse(new[]
            {
                "--phrase", ValidMnemonic,
                "--seed", seedBin,
                "--format", "bin"
            });

            Assert.That(result.IsSuccess, Is.True, result.Error);
            Assert.That(result.Value, Is.InstanceOf<VerifyCommand>());
        }

        [Test]
        public void Parse_NoArgs_ReturnsPhraseRequired()
        {
            var parser = new VerifyParser(_commandServiceMock.Object);

            var result = parser.Parse(Array.Empty<string>());

            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Error, Is.EqualTo("Option '--phrase' is required for 'verify'."));
        }

        [Test]
        public void Parse_PhraseMissingValue_ReturnsExactMessage()
        {
            var parser = new VerifyParser(_commandServiceMock.Object);

            var result = parser.Parse(new[] { "--phrase" });

            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Error, Is.EqualTo("Missing value for '--phrase'."));
        }

        [Test]
        public void Parse_SeedMissingValue_ReturnsExactMessage()
        {
            var parser = new VerifyParser(_commandServiceMock.Object);

            var result = parser.Parse(new[] { "--phrase", ValidMnemonic, "--seed" });

            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Error, Is.EqualTo("Missing value for '--seed'."));
        }

        [Test]
        public void Parse_SeedNotProvided_ReturnsRequiredMessage()
        {
            var parser = new VerifyParser(_commandServiceMock.Object);

            var result = parser.Parse(new[] { "--phrase", ValidMnemonic });

            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Error, Is.EqualTo("Option '--seed' is required for 'verify'."));
        }

        [Test]
        public void Parse_PhraseNotProvided_ReturnsRequiredMessage()
        {
            var parser = new VerifyParser(_commandServiceMock.Object);

            var result = parser.Parse(new[] { "--seed", ValidSeedHex, "--format", "hex" });

            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Error, Is.EqualTo("Option '--phrase' is required for 'verify'."));
        }

        [Test]
        public void Parse_UnknownOption_ReturnsExactMessage()
        {
            var parser = new VerifyParser(_commandServiceMock.Object);

            var result = parser.Parse(new[]
            {
                "--phrase", ValidMnemonic,
                "--seed", ValidSeedHex,
                "--wat", "hex"
            });

            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Error, Is.EqualTo("Unrecognized option '--wat' for 'verify'."));
        }

        [TestCase("HEX")]
        [TestCase("Bin")]
        [TestCase("unknown")]
        public void Parse_InvalidFormat_ReturnsExactMessage(string badFormat)
        {
            var parser = new VerifyParser(_commandServiceMock.Object);

            var result = parser.Parse(new[]
            {
                "--phrase", ValidMnemonic,
                "--seed", ValidSeedHex,
                "--format", badFormat
            });

            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Error, Is.EqualTo("Format must be 'hex' or 'bin'."));
        }

        [Test]
        public void Parse_InvalidHexSeedCharacters_ReturnsExactMessage()
        {
            var parser = new VerifyParser(_commandServiceMock.Object);
            var badSeed = new string('Z', 128);

            var result = parser.Parse(new[]
            {
                "--phrase", ValidMnemonic,
                "--seed", badSeed,
                "--format", "hex"
            });

            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Error, Is.EqualTo("Seed must be a valid hexadecimal string."));
        }

        [Test]
        public void Parse_InvalidHexSeedLength_ReturnsExactMessage()
        {
            var parser = new VerifyParser(_commandServiceMock.Object);

            var result = parser.Parse(new[]
            {
                "--phrase", ValidMnemonic,
                "--seed", "abcd",
                "--format", "hex"
            });

            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Error, Is.EqualTo("Hex seed must be exactly 128 characters long."));
        }

        [Test]
        public void Parse_InvalidBinarySeedCharacters_ReturnsExactMessage()
        {
            var parser = new VerifyParser(_commandServiceMock.Object);
            var badSeed = new string('2', 512);

            var result = parser.Parse(new[]
            {
                "--phrase", ValidMnemonic,
                "--seed", badSeed,
                "--format", "bin"
            });

            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Error, Is.EqualTo("Seed must be a valid binary string."));
        }

        [Test]
        public void Parse_InvalidBinarySeedLength_ReturnsExactMessage()
        {
            var parser = new VerifyParser(_commandServiceMock.Object);
            var shortSeed = new string('0', 511);

            var result = parser.Parse(new[]
            {
                "--phrase", ValidMnemonic,
                "--seed", shortSeed,
                "--format", "bin"
            });

            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Error, Is.EqualTo("Binary seed must be exactly 512 bits long."));
        }

        [Test]
        public void Parse_DefaultFormat_IsHex()
        {
            var parser = new VerifyParser(_commandServiceMock.Object);

            var result = parser.Parse(new[]
            {
                "--phrase", ValidMnemonic,
                "--seed", ValidSeedHex
            });

            Assert.That(result.IsSuccess, Is.True, result.Error);
            Assert.That(result.Value, Is.InstanceOf<VerifyCommand>());
        }
    }
}
