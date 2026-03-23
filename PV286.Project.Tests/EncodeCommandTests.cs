using Moq;
using BusinessLayer.CLI.Commands.Encode;
using BusinessLayer.CLI.Utils.Enums;
using BusinessLayer.Services.Interfaces;
using BusinessLayer.DTOs;
using NBitcoin;
using ResultPattern;

namespace PV286.Project.Tests
{
    [TestFixture]
    public class EncodeCommandTests
    {
        private Mock<ICommandService> _commandServiceMock = null!;

        [SetUp]
        public void Setup()
        {
            _commandServiceMock = new Mock<ICommandService>(MockBehavior.Strict);
        }

        // -------------------------------------------------------
        // 1) SUCCESS CASE
        // -------------------------------------------------------
        [Test]
        public void Handle_WhenEncodeSucceeds_WritesOutputAndReturnsTrue()
        {
            var entropy = new byte[] { 0x01, 0x02 };
            var format = ValueFormat.Hex;

            var dto = new EncodeDTO
            {
                Mnemonic = new Mnemonic(Wordlist.English, WordCount.Twelve),
                Seed = new byte[] { 0xAA, 0xBB },
                Format = format
            };

            _commandServiceMock
                .Setup(s => s.Encode(entropy, format))
                .Returns(Result.Ok(dto));

            var encodeCommand = new EncodeCommand(entropy, format, _commandServiceMock.Object);

            using var outWriter = new StringWriter();
            var origOut = Console.Out;
            Console.SetOut(outWriter);

            try
            {
                var ok = encodeCommand.Handle();

                Assert.That(ok, Is.True);

                string printed = outWriter.ToString();
                Assert.That(printed, Contains.Substring(dto.ToString()));

                _commandServiceMock.Verify(
                    s => s.Encode(entropy, format),
                    Times.Once
                );
            }
            finally
            {
                Console.SetOut(origOut);
            }
        }

        // -------------------------------------------------------
        // 2) FAILURE CASE
        // -------------------------------------------------------
        [Test]
        public void Handle_WhenEncodeFails_WritesErrorAndReturnsFalse()
        {
            var entropy = new byte[] { 0xFF };
            var format = ValueFormat.Bin;

            _commandServiceMock
                .Setup(s => s.Encode(entropy, format))
                .Returns(Result.Fail<EncodeDTO>("Something went wrong"));

            var encodeCommand = new EncodeCommand(entropy, format, _commandServiceMock.Object);

            using var errWriter = new StringWriter();
            var origErr = Console.Error;
            Console.SetError(errWriter);

            try
            {
                var ok = encodeCommand.Handle();

                Assert.That(ok, Is.False);

                string printed = errWriter.ToString();
                Assert.That(printed, Contains.Substring("Something went wrong"));

                _commandServiceMock.Verify(
                    s => s.Encode(entropy, format),
                    Times.Once
                );
            }
            finally
            {
                Console.SetError(origErr);
            }
        }

        // -------------------------------------------------------
        // 3) Constructor correctness
        // -------------------------------------------------------
        [Test]
        public void Constructor_SetsProperties()
        {
            var entropy = new byte[] { 0x11, 0x22 };
            var encodeCommand = new EncodeCommand(entropy, ValueFormat.Hex, _commandServiceMock.Object);

            Assert.That(encodeCommand.Entropy, Is.EqualTo(entropy));
            Assert.That(encodeCommand.Format, Is.EqualTo(ValueFormat.Hex));
        }
    }
}
