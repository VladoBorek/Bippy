using NUnit.Framework;
using BusinessLayer.Services;
using BusinessLayer.CLI.Utils.Enums;
using BusinessLayer.DTOs;
using NBitcoin;
using ResultPattern;

namespace PV286.Project.Tests
{
    [TestFixture]
    public class CommandServiceTests
    {
        // -------------------------------------------------------
        // 1) Provided entropy → deterministic output
        // -------------------------------------------------------
        [Test]
        public void Encode_WithProvidedEntropy_ReturnsExpectedMnemonicAndSeed()
        {
            // Arrange
            var svc = new CommandService();
            var entropy = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
                                       0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };
            const ValueFormat format = ValueFormat.Hex;

            var result = svc.Encode(entropy, format);

            Assert.That(result.IsSuccess, Is.True, result.Error);

            var dto = result.Value;
            Assert.That(dto, Is.Not.Null);

            var expectedMnemonic = new Mnemonic(Wordlist.English, entropy);
            Assert.That(dto.Mnemonic.ToString(), Is.EqualTo(expectedMnemonic.ToString()));

            var expectedSeed = expectedMnemonic.DeriveSeed();
            Assert.That(dto.Seed, Is.EqualTo(expectedSeed));

            Assert.That(dto.Format, Is.EqualTo(format));
        }

        // -------------------------------------------------------
        // 2) Null entropy → random generation (32 bytes)
        // -------------------------------------------------------
        [Test]
        public void Encode_NullEntropy_GeneratesRandomEntropyOfCorrectLength()
        {
            var svc = new CommandService();

            var result = svc.Encode(null, ValueFormat.Hex);

            Assert.That(result.IsSuccess, Is.True, result.Error);

            var dto = result.Value;
            Assert.That(dto, Is.Not.Null);

            // Entropy not directly exposed, but we can infer:
            //  - Seed comes from a mnemonic created with 32 bytes of entropy
            //  - NBitcoin stores entropy internally; word count reflects entropy size
            // 128 bits  (16 bytes) => 12 words
            // 160 bits  (20 bytes) => 15 words
            // 192 bits  (24 bytes) => 18 words
            // 224 bits  (28 bytes) => 21 words
            // 256 bits  (32 bytes) => 24 words
            Assert.That(dto.Mnemonic.Words.Length, Is.EqualTo(24),
                "24-word mnemonic implies 256-bit (32-byte) entropy as required.");
        }

        // -------------------------------------------------------
        // 3) Output DTO contains the same format that was passed in
        // -------------------------------------------------------
        [TestCase(ValueFormat.Hex)]
        [TestCase(ValueFormat.Bin)]
        public void Encode_PreservesFormat(ValueFormat format)
        {
            var svc = new CommandService();

            var result = svc.Encode(null, format);

            Assert.That(result.IsSuccess, Is.True, result.Error);
            Assert.That(result.Value!.Format, Is.EqualTo(format));
        }

        // -------------------------------------------------------
        // 4) Mnemonic and Seed are consistent (roundtrip check)
        // -------------------------------------------------------
        [Test]
        public void Encode_SeedMatchesDerivedSeed()
        {
            var svc = new CommandService();
            var entropy = RandomUtils.GetBytes(32);

            var result = svc.Encode(entropy, ValueFormat.Hex);
            Assert.That(result.IsSuccess, Is.True);

            var dto = result.Value!;
            var rederived = dto.Mnemonic.DeriveSeed();

            Assert.That(dto.Seed, Is.EqualTo(rederived));
        }
    }
}