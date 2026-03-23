using NUnit.Framework;
using BusinessLayer.CLI.Validators;
using BusinessLayer.CLI.Utils.Enums;

namespace PV286.Project.Tests;

[TestFixture]
public class EntropyValidatorTests
{
    // ------------------------------
    //  VALID HEX ENTROPY TESTS
    // ------------------------------
    [TestCase("00112233445566778899AABBCCDDEEFF")] // 32 chars
    [TestCase("00112233445566778899AABBCCDDEEFF00112233")] // 40 chars
    [TestCase("00112233445566778899AABBCCDDEEFF0011223344556677")] // 48 chars
    [TestCase("00112233445566778899AABBCCDDEEFF00112233445566778899AABB")] // 56 chars
    [TestCase("00112233445566778899AABBCCDDEEFF00112233445566778899AABBCCDDEEFF")] // 64 chars
    public void Hex_Valid_ReturnsOk(string validHex)
    {
        var result = EntropyValidator.IsValidEntropy(validHex, ValueFormat.Hex);

        Assert.That(result.IsSuccess, Is.True, result.Error);
    }

    // ------------------------------
    //  INVALID HEX ENTROPY TESTS
    // ------------------------------
    [TestCase("ZZZZZZZZ")]     // not hex
    [TestCase("ABCDEF")]       // too short (<32 chars)
    [TestCase("ABCDEF00ABC")]  // not divisible by 8
    [TestCase("A1")]           // tiny
    public void Hex_Invalid_ReturnsFail(string invalidHex)
    {
        var result = EntropyValidator.IsValidEntropy(invalidHex, ValueFormat.Hex);

        Assert.That(result.IsFailed, Is.True);
        Assert.That(result.Error, Does.Contain("hex").IgnoreCase);
    }

    // ------------------------------
    //  VALID BINARY ENTROPY TESTS
    // ------------------------------
    // Uses a generated string with only '0' characters of the correct length
    [TestCase(128)]
    [TestCase(160)]
    [TestCase(192)]
    [TestCase(224)]
    [TestCase(256)]
    public void Binary_ValidLengths_ReturnsOk(int bitLength)
    {
        string validBin = new string('0', bitLength);

        var result = EntropyValidator.IsValidEntropy(validBin, ValueFormat.Bin);

        Assert.That(result.IsSuccess, Is.True, result.Error);
    }

    // ------------------------------
    //  INVALID BINARY ENTROPY TESTS
    // ------------------------------
    [TestCase("010102")]  // contains invalid '2'
    [TestCase("0")]       // too short (<128 bits)
    [TestCase("00001111")] // 8 bits → too short
    [TestCase("0011")]     // too short
    [TestCase("00110011" + "0")] // 9 bits → not divisible by 32
    public void Binary_Invalid_ReturnsFail(string invalidBin)
    {
        var result = EntropyValidator.IsValidEntropy(invalidBin, ValueFormat.Bin);

        Assert.That(result.IsFailed, Is.True);
        Assert.That(result.Error, Does.Contain("binary").IgnoreCase);
    }
}