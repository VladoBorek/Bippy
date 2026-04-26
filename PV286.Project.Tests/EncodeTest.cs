using BusinessLayer.DTOs;
using BusinessLayer.Cli.Utils.Enums;
using NBitcoin;

namespace PV286.Project.Tests
{
    [TestFixture]
    public class EncodeDTests
    {
        private const string ExampleMnemonic =
            "abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon about";

        [Test]
        public void ToString_HexFormat_IncludesLowercaseHexSeed_AndMnemonic()
        {
            // Arrange
            var dto = new EncodeDTO
            {
                Mnemonic = new Mnemonic(ExampleMnemonic, Wordlist.English),
                Seed = new byte[] { 0x0A, 0x1B }, // hex -> "0a1b"
                Format = ValueFormat.Hex
            };

            // Act
            var text = dto.ToString();

            // Assert (modern NUnit constraint syntax)
            Assert.That(text, Does.Contain("Mnemonic : "));
            Assert.That(text, Does.Contain(ExampleMnemonic));

            Assert.That(text, Does.Contain("Seed     : "));
            Assert.That(text, Does.Contain("0a1b")); // lowercase hex

            Assert.That(text, Does.Contain(Environment.NewLine));
        }

        [Test]
        public void ToString_BinFormat_Concatenates8BitBinary_AndIncludesMnemonic()
        {
            var dto = new EncodeDTO
            {
                Mnemonic = new Mnemonic(ExampleMnemonic, Wordlist.English),
                Seed = new byte[] { 0x0A, 0x1B }, // 00001010 00011011
                Format = ValueFormat.Bin
            };

            var text = dto.ToString();

            Assert.That(text, Does.Contain("Mnemonic : "));
            Assert.That(text, Does.Contain(ExampleMnemonic));

            Assert.That(text, Does.Contain("Seed     : "));

            const string expectedBinary = "00001010" + "00011011";
            Assert.That(text, Does.Contain(expectedBinary));
        }

        [Test]
        public void Properties_AreAssignedCorrectly()
        {
            var mnemonic = new Mnemonic(ExampleMnemonic, Wordlist.English);
            var seed = Enumerable.Range(0, 4).Select(i => (byte)i).ToArray();

            var dto = new EncodeDTO
            {
                Mnemonic = mnemonic,
                Seed = seed,
                Format = ValueFormat.Hex
            };

            Assert.That(dto.Mnemonic, Is.SameAs(mnemonic));
            Assert.That(dto.Seed, Is.EqualTo(seed));
            Assert.That(dto.Format, Is.EqualTo(ValueFormat.Hex));
        }
    }
}