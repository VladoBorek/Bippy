using NUnit.Framework;
using BusinessLayer.CLI.Validators;
using BusinessLayer.CLI.Utils.Enums;

namespace PV286.Project.Tests
{
    [TestFixture]
    public class EncodeValidatorTests
    {
        [Test]
        public void NoEntropy_FormatIrrelevant_ReturnsOk()
        {
            // Arrange & Act
            var result = EncodeValidator.IsValidEncode(
                entropy: null,
                formatProvided: false,   
                format: ValueFormat.Hex  
            );

            Assert.That(result.IsSuccess, Is.True, result.Error);
        }

        [Test]
        public void EntropyProvided_ButNoFormatProvided_ReturnsFailWithExactMessage()
        {
            var result = EncodeValidator.IsValidEncode(
                entropy: "00",
                formatProvided: false,
                format: ValueFormat.Hex
            );

            // Assert
            Assert.That(result.IsFailed, Is.True);
            Assert.That(
                result.Error,
                Is.EqualTo("Option '--format' is required when '--entropy' is provided.")
            );
        }

        [Test]
        public void EntropyProvided_WithFormat_AndValidHex_ReturnsOk()
        {
            const string validHex32 = "00112233445566778899AABBCCDDEEFF";

            // Act
            var result = EncodeValidator.IsValidEncode(
                entropy: validHex32,
                formatProvided: true,
                format: ValueFormat.Hex
            );

            // Assert
            Assert.That(result.IsSuccess, Is.True, result.Error);
        }
        [Test]
        public void EntropyProvided_WithFormat_AndValidBin_ReturnsOk()
        {
            const string validBin128 =
                "0001001000110100010101100111100010011010101111001101111011110000" + 
                "1111000011110000111100001111000011110000111100001111000011110000"; 

            var result = EncodeValidator.IsValidEncode(
                entropy: validBin128,
                formatProvided: true,
                format: ValueFormat.Bin
            );

            Assert.That(result.IsSuccess, Is.True, result.Error);
        }
    }
}
