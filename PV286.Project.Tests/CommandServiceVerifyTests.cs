using BusinessLayer.DTOs;
using BusinessLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PV286.Project.Tests
{
    [TestFixture]
    public class CommandServiceVerifyTests
    {
        private const string ValidMnemonic =
            "photo memory captain decline vendor heavy seminar gloom mouse economy awkward tilt";

        private const string ValidSeedHex =
            "f337beabcfff42915cd9a65fb48745dd0bd122718f04789caeed86cacd35c028" +
            "d151f3f1ed100bf01adedbf734270b269632d851f45cbbf4bdd0ba6c0bce94db";

        [Test]
        public void Verify_ValidPhraseAndMatchingSeed_ReturnsOkWithTrue()
        {
            var service = new VerifyService();
            var seed = Convert.FromHexString(ValidSeedHex);

            var result = service.Verify(ValidMnemonic, seed);

            Assert.That(result.IsSuccess, Is.True, result.Error);
            Assert.That(result.Value, Is.InstanceOf<VerifyDTO>());
            Assert.That(result.Value.IsValid, Is.True);
        }

        [Test]
        public void Verify_ValidPhraseAndDifferentSeed_ReturnsOkWithFalse()
        {
            var service = new VerifyService();
            var seed = Convert.FromHexString(ValidSeedHex);
            seed[0] ^= 0xFF;

            var result = service.Verify(ValidMnemonic, seed);

            Assert.That(result.IsSuccess, Is.True, result.Error);
            Assert.That(result.Value.IsValid, Is.False);
        }

        [Test]
        public void Verify_ValidPhraseAndAllZeroSeed_ReturnsOkWithFalse()
        {
            var service = new VerifyService();
            var seed = new byte[64];

            var result = service.Verify(ValidMnemonic, seed);

            Assert.That(result.IsSuccess, Is.True, result.Error);
            Assert.That(result.Value.IsValid, Is.False);
        }

        [Test]
        public void Verify_InvalidWord_Fails()
        {
            var service = new VerifyService();
            var badMnemonic =
                "photo memory captain decline vendor heavy seminar gloom mouse economy awkward WRONGWORD";
            var seed = Convert.FromHexString(ValidSeedHex);

            var result = service.Verify(badMnemonic, seed);

            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Error, Does.Contain("Invalid mnemonic").IgnoreCase);
        }

        [Test]
        public void Verify_BadChecksum_Fails()
        {
            var service = new VerifyService();
            var badMnemonic =
                "photo memory captain decline vendor heavy seminar gloom mouse economy awkward atom";
            var seed = Convert.FromHexString(ValidSeedHex);

            var result = service.Verify(badMnemonic, seed);

            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Error, Does.Contain("checksum").IgnoreCase);
        }

        [Test]
        public void Verify_WrongWordCount_Fails()
        {
            var service = new VerifyService();
            var badMnemonic =
                "photo memory captain decline vendor heavy seminar gloom mouse economy awkward";
            var seed = Convert.FromHexString(ValidSeedHex);

            var result = service.Verify(badMnemonic, seed);

            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Error, Does.Contain("Invalid mnemonic").IgnoreCase);
        }

        [Test]
        public void Verify_EmptyPhrase_Fails()
        {
            var service = new VerifyService();
            var seed = Convert.FromHexString(ValidSeedHex);

            var result = service.Verify(string.Empty, seed);

            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Error, Does.Contain("Invalid mnemonic").IgnoreCase);
        }

        [Test]
        public void Verify_SameInputs_IsDeterministic()
        {
            var service = new VerifyService();
            var seed = Convert.FromHexString(ValidSeedHex);

            var result1 = service.Verify(ValidMnemonic, seed);
            var result2 = service.Verify(ValidMnemonic, seed);

            Assert.That(result1.IsSuccess, Is.True, result1.Error);
            Assert.That(result2.IsSuccess, Is.True, result2.Error);
            Assert.That(result1.Value.IsValid, Is.True);
            Assert.That(result2.Value.IsValid, Is.True);
        }
    }
}
