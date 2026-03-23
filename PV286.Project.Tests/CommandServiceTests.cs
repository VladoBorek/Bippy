using BusinessLayer.Services;
using BusinessLayer.CLI.Utils.Enums;
using NBitcoin;


namespace PV286.Project.Tests
{
    [TestFixture]
    public class CommandServiceMnemonicSizeTests
    {
        [Test]
        public void Entropy_16Bytes_Produces12Words()
        {
            var svc = new CommandService();
            var entropy = new byte[16]; // 128 bits
            var res = svc.Encode(entropy, ValueFormat.Hex);

            Assert.That(res.IsSuccess, Is.True, res.Error);
            Assert.That(res.Value.Mnemonic.Words.Length, Is.EqualTo(12));
        }

        [Test]
        public void Entropy_20Bytes_Produces15Words()
        {
            var svc = new CommandService();
            var entropy = new byte[20]; // 160 bits
            var res = svc.Encode(entropy, ValueFormat.Hex);

            Assert.That(res.IsSuccess, Is.True, res.Error);
            Assert.That(res.Value.Mnemonic.Words.Length, Is.EqualTo(15));
        }

        [Test]
        public void Entropy_24Bytes_Produces18Words()
        {
            var svc = new CommandService();
            var entropy = new byte[24]; // 192 bits
            var res = svc.Encode(entropy, ValueFormat.Hex);

            Assert.That(res.IsSuccess, Is.True, res.Error);
            Assert.That(res.Value.Mnemonic.Words.Length, Is.EqualTo(18));
        }

        [Test]
        public void Entropy_28Bytes_Produces21Words()
        {
            var svc = new CommandService();
            var entropy = new byte[28]; // 224 bits
            var res = svc.Encode(entropy, ValueFormat.Hex);

            Assert.That(res.IsSuccess, Is.True, res.Error);
            Assert.That(res.Value.Mnemonic.Words.Length, Is.EqualTo(21));
        }

        [Test]
        public void Entropy_32Bytes_Produces24Words()
        {
            var svc = new CommandService();
            var entropy = new byte[32]; // 256 bits
            var res = svc.Encode(entropy, ValueFormat.Hex);

            Assert.That(res.IsSuccess, Is.True, res.Error);
            Assert.That(res.Value.Mnemonic.Words.Length, Is.EqualTo(24));
        }
    }

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
            var commandService = new CommandService();
            var entropy = new byte[]
            {
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
                0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F
            };
            const ValueFormat format = ValueFormat.Hex;

            var result = commandService.Encode(entropy, format);

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
            var commandService = new CommandService();

            var result = commandService.Encode(null, ValueFormat.Hex);

            Assert.That(result.IsSuccess, Is.True, result.Error);

            var dto = result.Value;
            Assert.That(dto, Is.Not.Null);
            
            Assert.That(dto.Mnemonic.Words.Length, Is.EqualTo(24),
                "24-word mnemonic implies 256-bit (32-byte) entropy as required.");
        }
        // Override behavior for this test only

        // -------------------------------------------------------
        // 3) Output DTO contains the same format that was passed in
        // -------------------------------------------------------
        [TestCase(ValueFormat.Hex)]
        [TestCase(ValueFormat.Bin)]
        public void Encode_PreservesFormat(ValueFormat format)
        {
            var commandService = new CommandService();

            var result = commandService.Encode(null, format);

            Assert.That(result.IsSuccess, Is.True, result.Error);
            Assert.That(result.Value.Format, Is.EqualTo(format));
        }

        // -------------------------------------------------------
        // 4) Mnemonic and Seed are consistent (roundtrip check)
        // -------------------------------------------------------
        [Test]
        public void Encode_SeedMatchesDerivedSeed()
        {
            var commandService = new CommandService();
            var entropy = RandomUtils.GetBytes(32);

            var result = commandService.Encode(entropy, ValueFormat.Hex);
            Assert.That(result.IsSuccess, Is.True);

            var dto = result.Value;
            var rederived = dto.Mnemonic.DeriveSeed();

            Assert.That(dto.Seed, Is.EqualTo(rederived));
        }
    }
}