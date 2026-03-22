using NUnit.Framework;
using BusinessLayer.CLI.Validators;
using BusinessLayer.CLI.Utils.Enums;
using ResultPattern;

namespace PV286.Project.Tests
{
    [TestFixture]
    public class EncodeValidatorTests
    {

        [Test]
        public void NoEntropy_FormatNotProvided_IsOk()
        {
            var result = EncodeValidator.IsValidEncode(
                entropy: null,
                formatProvided: false,
                format: ValueFormat.Hex  // value ignored when entropy is null
            );

            Assert.That(result.IsSuccess, Is.True, result.Error);
        }
    }
}