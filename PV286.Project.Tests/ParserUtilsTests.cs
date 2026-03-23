using BusinessLayer.CLI.Utils;
using BusinessLayer.CLI.Utils.Enums;

namespace PV286.Project.Tests;

[TestFixture]
public class ParserUtilsTests
{
    // ================================
    //  GetRequiredValue tests
    // ================================

    [Test]
    public void GetRequiredValue_ValuePresent_ReturnsOk()
    {
        string[] args = { "--entropy", "A1B2" };
        int index = 0;

        var result = ParserUtils.GetRequiredValue(args, ref index, "--entropy");

        Assert.That(result.IsSuccess, Is.True, result.Error);
        Assert.That(result.Value, Is.EqualTo("A1B2"));
        Assert.That(index, Is.EqualTo(1));
    }

    [Test]
    public void GetRequiredValue_MissingValue_ReturnsFail()
    {
        string[] args = { "--entropy" };
        int index = 0;

        var result = ParserUtils.GetRequiredValue(args, ref index, "--entropy");

        Assert.That(result.IsFailed, Is.True);
        Assert.That(result.Error, Is.EqualTo("Missing value for '--entropy'."));
    }

    [Test]
    public void GetRequiredValue_NextArgIsAnotherOption_ReturnsFail()
    {
        string[] args = { "--entropy", "--format" };
        int index = 0;

        var result = ParserUtils.GetRequiredValue(args, ref index, "--entropy");

        Assert.That(result.IsFailed, Is.True);
        Assert.That(result.Error, Is.EqualTo("Missing value for '--entropy'."));
        Assert.That(index, Is.EqualTo(0)); 
    }

    [Test]
    public void ParseFormat_Hex_ReturnsHexFormat()
    {
        var result = ParserUtils.ParseFormat("hex");

        Assert.That(result.IsSuccess, Is.True, result.Error);
        Assert.That(result.Value, Is.EqualTo(ValueFormat.Hex));
    }

    [Test]
    public void ParseFormat_Bin_ReturnsBinFormat()
    {
        var result = ParserUtils.ParseFormat("bin");

        Assert.That(result.IsSuccess, Is.True, result.Error);
        Assert.That(result.Value, Is.EqualTo(ValueFormat.Bin));
    }

    [TestCase("HEX")]
    [TestCase("Bin")]
    [TestCase("Hex")]
    [TestCase("BiN")]
    public void ParseFormat_IsCaseSensitive_UppercaseFails(string input)
    {
        var result = ParserUtils.ParseFormat(input);

        Assert.That(result.IsFailed, Is.True);
        Assert.That(result.Error, Is.EqualTo("Format must be 'hex' or 'bin'."));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("xyz")]
    [TestCase("base64")]
    [TestCase("binary")]
    public void ParseFormat_InvalidStrings_ReturnsFail(string input)
    {
        var result = ParserUtils.ParseFormat(input);

        Assert.That(result.IsFailed, Is.True);
        Assert.That(result.Error, Is.EqualTo("Format must be 'hex' or 'bin'."));
    }
}