using BusinessLayer.Cli.Commands.Encode;
using BusinessLayer.Cli.Utils.Enums;
using BusinessLayer.Services.Interfaces;
using Moq;

namespace PV286.Project.Tests
{
    [TestFixture]
    public class EncodeParserTests
    {
        private Mock<IEncodeService> _encodeServiceMock = null!;

        [SetUp]
        public void SetUp()
        {
            _encodeServiceMock = new Mock<IEncodeService>(MockBehavior.Strict);
        }

        // -------------------------------------------------------
        // 1) No args -> returns EncodeCommand (no entropy, default HEX)
        // -------------------------------------------------------
        [Test]
        public void Parse_NoArgs_ReturnsEncodeCommand_WithDefaultHex_AndNullEntropy()
        {
            var encodeParser = new EncodeParser(_encodeServiceMock.Object);

            var result = encodeParser.Parse(Array.Empty<string>());

            Assert.That(result.IsSuccess, Is.True, result.Error);
            Assert.That(result.Value, Is.InstanceOf<EncodeCommand>());

            var cmd = (EncodeCommand)result.Value;
            Assert.That(cmd.Format, Is.EqualTo(ValueFormat.Hex));
            Assert.That(cmd.Entropy, Is.Null);
        }

        // -------------------------------------------------------
        // 2) Unknown option -> fail with exact message
        // -------------------------------------------------------
        [Test]
        public void Parse_UnknownOption_ReturnsFail_WithExactMessage()
        {
            var encodeParser = new EncodeParser(_encodeServiceMock.Object);

            var result = encodeParser.Parse(new[] { "--unknown" });

            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Error, Is.EqualTo("Unrecognized option '--unknown' for 'encode'."));
        }

        // -------------------------------------------------------
        // 3) --entropy with missing value -> fail (message from ParserUtils)
        // -------------------------------------------------------
        [Test]
        public void Parse_EntropyMissingValue_ReturnsFail_WithExactMessage()
        {
            var encodeParser = new EncodeParser(_encodeServiceMock.Object);

            var result = encodeParser.Parse(new[] { "--entropy" });

            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Error, Is.EqualTo("Missing value for '--entropy'."));
        }

        // -------------------------------------------------------
        // 4) --entropy <value> without --format -> fail (message from EncodeValidator)
        // -------------------------------------------------------
        [Test]
        public void Parse_EntropyProvided_WithoutFormat_ReturnsFail_WithExactMessage()
        {
            // valid 32-hex-char value so only "format missing" rule triggers
            const string validHex32 = "00112233445566778899AABBCCDDEEFF";
            var encodeParser = new EncodeParser(_encodeServiceMock.Object);

            var result = encodeParser.Parse(new[] { "--entropy", validHex32 });

            Assert.That(result.IsFailed, Is.True);
            Assert.That(
                result.Error,
                Is.EqualTo("Option '--format' is required when '--entropy' is provided.")
            );
        }

        // -------------------------------------------------------
        // 5) --entropy <hex> --format hex -> success; correct bytes & format
        // -------------------------------------------------------
        [Test]
        public void Parse_HexEntropy_WithHexFormat_ReturnsEncodeCommand_WithExpectedBytes()
        {
            const string hex = "00112233445566778899AABBCCDDEEFF"; // 32 chars
            var expectedBytes = Convert.FromHexString(hex);

            var encodeParser = new EncodeParser(_encodeServiceMock.Object);

            var result = encodeParser.Parse(new[] { "--entropy", hex, "--format", "hex" });

            Assert.That(result.IsSuccess, Is.True, result.Error);
            var cmd = result.Value as EncodeCommand;
            Assert.That(cmd, Is.Not.Null);

            Assert.That(cmd!.Format, Is.EqualTo(ValueFormat.Hex));
            Assert.That(cmd.Entropy, Is.EqualTo(expectedBytes));
        }

        // -------------------------------------------------------
        // 6) --entropy <bin> --format bin -> success; correct bytes & format
        // -------------------------------------------------------
        [Test]
        public void Parse_BinaryEntropy_WithBinFormat_ReturnsEncodeCommand_WithExpectedBytes()
        {
            // 128-bit binary: 16 bytes of 00000000 => all zeros
            var bin128 = new string('0', 128);
            var expected = Enumerable.Repeat((byte)0x00, 16).ToArray();

            var encodeParser = new EncodeParser(_encodeServiceMock.Object);

            var result = encodeParser.Parse(new[] { "--entropy", bin128, "--format", "bin" });

            Assert.That(result.IsSuccess, Is.True, result.Error);
            var cmd = result.Value as EncodeCommand;
            Assert.That(cmd, Is.Not.Null);

            Assert.That(cmd!.Format, Is.EqualTo(ValueFormat.Bin));
            Assert.That(cmd.Entropy, Is.EqualTo(expected));
        }

        // -------------------------------------------------------
        // 7) --format with invalid value (case-sensitive parser) -> fail
        // -------------------------------------------------------
        [TestCase("HEX")]
        [TestCase("Bin")]
        [TestCase("Hex")]
        public void Parse_FormatInvalidCase_ReturnsFail_WithExactMessage(string badFormat)
        {
            var encodeParser = new EncodeParser(_encodeServiceMock.Object);

            var result = encodeParser.Parse(new[] { "--format", badFormat });

            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Error, Is.EqualTo("Format must be 'hex' or 'bin'."));
        }
    }
}
