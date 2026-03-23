using Moq;
using BusinessLayer.CLI.Parser;
using BusinessLayer.CLI.Commands;
using BusinessLayer.CLI.Commands.Help;
using ResultPattern;

namespace PV286.Project.Tests
{
    [TestFixture]
    public class ArgParserTests
    {
        private Mock<ICliParser> _encodeParserMock = null!;
        private Mock<ICliParser> _otherParserMock = null!;
        private Mock<ICliCommand> _cmdMock = null!;
        private ArgParser _sut = null!;
        private string[]? _receivedArgs;

        [SetUp]
        public void SetUp()
        {
            _receivedArgs = null;

            // Fake command returned by the encode parser
            _cmdMock = new Mock<ICliCommand>();
            _cmdMock.SetupGet(c => c.CommandName).Returns("dummy");
            _cmdMock.Setup(c => c.Handle()).Returns(true);

            // Encode parser mock (expected to be called)
            _encodeParserMock = new Mock<ICliParser>();
            _encodeParserMock.SetupGet(p => p.CommandName).Returns("encode");
            _encodeParserMock
                .Setup(p => p.Parse(It.IsAny<string[]>()))
                .Callback((string[] args) => _receivedArgs = args)
                .Returns(() => Result.Ok(_cmdMock.Object));

            // Another parser that must NOT be called
            _otherParserMock = new Mock<ICliParser>();
            _otherParserMock.SetupGet(p => p.CommandName).Returns("other");
            _otherParserMock
                .Setup(p => p.Parse(It.IsAny<string[]>()))
                .Returns(Result.Fail<ICliCommand>("should not be called"));

            // System Under Test (ArgParser)
            _sut = new ArgParser(new[] 
            { 
                _otherParserMock.Object, 
                _encodeParserMock.Object 
            });
        }

        // ------------------------------ 1. Help cases ------------------------------
        [Test]
        public void Parse_NoArgs_ReturnsHelpCommand()
        {
            var result = _sut.Parse(Array.Empty<string>());

            Assert.That(result.IsSuccess, Is.True, result.Error);
            Assert.That(result.Value, Is.InstanceOf<HelpCommand>());
        }

        [TestCase("--help")]
        public void Parse_HelpFlag_ReturnsHelpCommand(string flag)
        {
            var result = _sut.Parse(new[] { flag });

            Assert.That(result.IsSuccess, Is.True, result.Error);
            Assert.That(result.Value, Is.InstanceOf<HelpCommand>());
        }

        // ------------------------------ 2. Unknown command ------------------------------
        [Test]
        public void Parse_UnknownCommand_ReturnsFailWithExactMessage()
        {
            var result = _sut.Parse(new[] { "does-not-exist" });

            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Error, Is.EqualTo("Unrecognized command 'does-not-exist'."));
        }

        // ------------------------------ 3. Successful delegation ------------------------------
        [Test]
        public void Parse_KnownCommand_DelegatesAndReturnsParserResult()
        {
            var input = new[] { "encode", "--format", "hex", "--entropy", "A1B2" };

            var result = _sut.Parse(input);

            Assert.That(result.IsSuccess, Is.True, result.Error);
            Assert.That(result.Value, Is.SameAs(_cmdMock.Object));

            Assert.That(_receivedArgs,
                Is.EqualTo(new[] { "--format", "hex", "--entropy", "A1B2" }));

            _encodeParserMock.Verify(p => p.Parse(It.IsAny<string[]>()), Times.Once);
            _otherParserMock.Verify(p => p.Parse(It.IsAny<string[]>()), Times.Never);
        }

        // ------------------------------ 4. Delegation failure ------------------------------
        [Test]
        public void Parse_KnownCommand_WhenParserFails_PropagatesFailure()
        {
            _encodeParserMock
                .Setup(p => p.Parse(It.IsAny<string[]>()))
                .Returns(Result.Fail<ICliCommand>("encode parse failed"));

            var result = _sut.Parse(new[] { "encode", "--bad", "value" });

            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Error, Is.EqualTo("encode parse failed"));

            _encodeParserMock.Verify(p => p.Parse(It.IsAny<string[]>()), Times.Once);
        }
    }
}