using System;
using System.IO;
using Moq;
using NUnit.Framework;
using BusinessLayer.CLI.Commands.Encode;
using BusinessLayer.CLI.Utils.Enums;
using BusinessLayer.Services.Interfaces;
using BusinessLayer.DTOs;
using NBitcoin;
using ResultPattern;

namespace PV286.Project.Tests.CLI.Commands
{
    [TestFixture]
    public class EncodeCommandTests
    {
        private Mock<ICommandService> commandServiceMock = null!;

        [SetUp]
        public void Setup()
        {
            commandServiceMock = new Mock<ICommandService>(MockBehavior.Strict);
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

            commandServiceMock
                .Setup(s => s.Encode(entropy, format))
                .Returns(Result.Ok(dto));

            var sut = new EncodeCommand(entropy, format, commandServiceMock.Object);

            using var outWriter = new StringWriter();
            var origOut = Console.Out;
            Console.SetOut(outWriter);

            try
            {
                var ok = sut.Handle();

                Assert.That(ok, Is.True);

                string printed = outWriter.ToString();
                Assert.That(printed, Contains.Substring(dto.ToString()));

                commandServiceMock.Verify(
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

            commandServiceMock
                .Setup(s => s.Encode(entropy, format))
                .Returns(Result.Fail<EncodeDTO>("Something went wrong"));

            var sut = new EncodeCommand(entropy, format, commandServiceMock.Object);

            using var errWriter = new StringWriter();
            var origErr = Console.Error;
            Console.SetError(errWriter);

            try
            {
                var ok = sut.Handle();

                Assert.That(ok, Is.False);

                string printed = errWriter.ToString();
                Assert.That(printed, Contains.Substring("Something went wrong"));

                commandServiceMock.Verify(
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
            var sut = new EncodeCommand(entropy, ValueFormat.Hex, commandServiceMock.Object);

            Assert.That(sut.Entropy, Is.EqualTo(entropy));
            Assert.That(sut.Format, Is.EqualTo(ValueFormat.Hex));
        }
    }
}
