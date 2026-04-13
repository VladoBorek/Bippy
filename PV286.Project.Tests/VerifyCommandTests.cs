using BusinessLayer.Cli.Commands.Verify;
using BusinessLayer.DTOs;
using BusinessLayer.Services.Interfaces;
using Moq;
using ResultPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PV286.Project.Tests
{
    [TestFixture]
    public class VerifyCommandTests
    {
        private Mock<IVerifyService> _verifyServiceMock = null!;

        private const string Phrase =
            "photo memory captain decline vendor heavy seminar gloom mouse economy awkward tilt";

        private static readonly byte[] Seed = Convert.FromHexString(
            "f337beabcfff42915cd9a65fb48745dd0bd122718f04789caeed86cacd35c028" +
            "d151f3f1ed100bf01adedbf734270b269632d851f45cbbf4bdd0ba6c0bce94db"
        );

        [SetUp]
        public void Setup()
        {
            _verifyServiceMock = new Mock<IVerifyService>(MockBehavior.Strict);
        }

        [Test]
        public void Handle_WhenVerifyReturnsOk_WritesOKAndReturnsTrue()
        {
            _verifyServiceMock
                .Setup(s => s.Verify(Phrase, Seed))
                .Returns(Result.Ok(new VerifyDTO { IsValid = true }));

            var cmd = new VerifyCommand(Phrase, Seed, _verifyServiceMock.Object);

            using var outWriter = new StringWriter();
            var origOut = Console.Out;
            Console.SetOut(outWriter);

            try
            {
                var ok = cmd.Handle();

                Assert.That(ok, Is.True);
                Assert.That(outWriter.ToString(), Does.Contain("OK"));

                _verifyServiceMock.Verify(s => s.Verify(Phrase, Seed), Times.Once);
            }
            finally
            {
                Console.SetOut(origOut);
            }
        }

        [Test]
        public void Handle_WhenVerifyReturnsNok_WritesNOKAndReturnsTrue()
        {
            _verifyServiceMock
                .Setup(s => s.Verify(Phrase, Seed))
                .Returns(Result.Ok(new VerifyDTO { IsValid = false }));

            var cmd = new VerifyCommand(Phrase, Seed, _verifyServiceMock.Object);

            using var outWriter = new StringWriter();
            var origOut = Console.Out;
            Console.SetOut(outWriter);

            try
            {
                var ok = cmd.Handle();

                Assert.That(ok, Is.True);
                Assert.That(outWriter.ToString(), Does.Contain("NOK"));

                _verifyServiceMock.Verify(s => s.Verify(Phrase, Seed), Times.Once);
            }
            finally
            {
                Console.SetOut(origOut);
            }
        }

        [Test]
        public void Handle_WhenVerifyFails_WritesErrorAndReturnsFalse()
        {
            _verifyServiceMock
                .Setup(s => s.Verify(Phrase, Seed))
                .Returns(Result.Fail<VerifyDTO>("Invalid mnemonic checksum."));

            var cmd = new VerifyCommand(Phrase, Seed, _verifyServiceMock.Object);

            using var errWriter = new StringWriter();
            var origErr = Console.Error;
            Console.SetError(errWriter);

            try
            {
                var ok = cmd.Handle();

                Assert.That(ok, Is.False);
                Assert.That(errWriter.ToString(), Does.Contain("Invalid mnemonic checksum."));

                _verifyServiceMock.Verify(s => s.Verify(Phrase, Seed), Times.Once);
            }
            finally
            {
                Console.SetError(origErr);
            }
        }
    }
}
