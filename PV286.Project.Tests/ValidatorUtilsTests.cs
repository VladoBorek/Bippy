using NUnit.Framework;
using BusinessLayer.CLI.Validators;

namespace PV286.Project.Tests.Validators;

[TestFixture]
public class ValidatorUtilsTests
{
    // ===========================
    // IsHex tests
    // ===========================

    [TestCase("0",  ExpectedResult = true)]
    [TestCase("F",  ExpectedResult = true)]
    [TestCase("a",  ExpectedResult = true)]
    [TestCase("0123456789abcdef", ExpectedResult = true)]
    [TestCase("0123456789ABCDEF", ExpectedResult = true)]
    [TestCase("A1b2C3d4E5f6",     ExpectedResult = true)]
    public bool IsHex_Valid_ReturnsTrue(string input)
        => ValidatorUtils.IsHex(input);

    [TestCase(null,     ExpectedResult = false)]
    [TestCase("",       ExpectedResult = false)]
    [TestCase(" ",     ExpectedResult = false)]
    [TestCase(" 0A ",   ExpectedResult = false)] 
    [TestCase("0xFF",   ExpectedResult = false)] 
    [TestCase("G",      ExpectedResult = false)]
    [TestCase("Z123",   ExpectedResult = false)]
    [TestCase("😅",     ExpectedResult = false)]
    [TestCase("AB CD",  ExpectedResult = false)]
    [TestCase("AA\nBB", ExpectedResult = false)]
    [TestCase("A_B",    ExpectedResult = false)]
    public bool IsHex_Invalid_ReturnsFalse(string? input)
        => ValidatorUtils.IsHex(input!);

    [Test]
    public void IsHex_LongValidString_ReturnsTrue()
    {
        // 64 hex chars
        const string hex64 = "00112233445566778899AABBCCDDEEFF00112233445566778899AABBCCDDEEFF";
        Assert.That(ValidatorUtils.IsHex(hex64), Is.True);
    }

    // ===========================
    // IsBinary tests
    // ===========================

    [TestCase("0",            ExpectedResult = true)]
    [TestCase("1",            ExpectedResult = true)]
    [TestCase("01",           ExpectedResult = true)]
    [TestCase("00110011",     ExpectedResult = true)]
    [TestCase("00000000",     ExpectedResult = true)]
    [TestCase("11111111",     ExpectedResult = true)]
    public bool IsBinary_Valid_ReturnsTrue(string input)
        => ValidatorUtils.IsBinary(input);

    [TestCase(null,     ExpectedResult = false)]
    [TestCase("",       ExpectedResult = false)]
    [TestCase("2",      ExpectedResult = false)]
    [TestCase("0102",   ExpectedResult = false)]
    [TestCase("01 01",  ExpectedResult = false)] 
    [TestCase("01\n01", ExpectedResult = false)] 
    [TestCase("01x01",  ExpectedResult = false)] 
    [TestCase("😅",     ExpectedResult = false)]
    public bool IsBinary_Invalid_ReturnsFalse(string? input)
        => ValidatorUtils.IsBinary(input!);

    [Test]
    public void IsBinary_LongValidString_ReturnsTrue()
    {
        string bin128 = new string('0', 128);
        Assert.That(ValidatorUtils.IsBinary(bin128), Is.True);
    }
}