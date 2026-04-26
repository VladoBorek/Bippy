using BusinessLayer.Cli.Commands;
using BusinessLayer.Cli.Commands.Help;
using BusinessLayer.Cli.Parser;
using BusinessLayer.Cli.Utils.Parser;
using Moq;
using ResultPattern;

namespace PV286.Project.Tests
{
    [TestFixture]
    public class ArgParserTests
    {
        private class FakeCmdParser : CmdParser
        {
            private readonly string _name;
            public Result<ICliCommand> ParseResult { get; set; }
            public bool WasCalled { get; private set; }

            public FakeCmdParser(string name)
            {
                _name = name;
                ParseResult = Result.Fail<ICliCommand>("default");
            }

            public override string CommandName => _name;

            protected override FlagParser FlagParser() => new FlagParser(_name)
                .Add("--format")
                .Add("--entropy")
                .Add("--bad");

            protected override Result<ICliCommand> Build(ParsedArgs opts)
            {
                WasCalled = true;
                return ParseResult;
            }
        }

        private FakeCmdParser _encodeParserFake = null!;
        private FakeCmdParser _otherParserFake = null!;
        private Mock<ICliCommand> _cmdMock = null!;
        private ArgParser _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _cmdMock = new Mock<ICliCommand>();
            _cmdMock.SetupGet(c => c.CommandName).Returns("dummy");
            _cmdMock.Setup(c => c.Handle()).Returns(true);

            _encodeParserFake = new FakeCmdParser("encode")
            {
                ParseResult = Result.Ok(_cmdMock.Object)
            };

            _otherParserFake = new FakeCmdParser("other")
            {
                ParseResult = Result.Fail<ICliCommand>("should not be called")
            };

            _sut = new ArgParser(new CmdParser[]
            {
                _otherParserFake,
                _encodeParserFake
            });
        }

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

        [Test]
        public void Parse_UnknownCommand_ReturnsFailWithExactMessage()
        {
            var result = _sut.Parse(new[] { "does-not-exist" });
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Error, Is.EqualTo("Unrecognized command 'does-not-exist'."));
        }

        [Test]
        public void Parse_KnownCommand_DelegatesAndReturnsParserResult()
        {
            var input = new[] { "encode", "--format", "hex", "--entropy", "A1B2" };
            var result = _sut.Parse(input);

            Assert.That(result.IsSuccess, Is.True, result.Error);
            Assert.That(result.Value, Is.SameAs(_cmdMock.Object));
            Assert.That(_encodeParserFake.WasCalled, Is.True);
            Assert.That(_otherParserFake.WasCalled, Is.False);
        }

        [Test]
        public void Parse_KnownCommand_WhenParserFails_PropagatesFailure()
        {
            _encodeParserFake.ParseResult = Result.Fail<ICliCommand>("encode parse failed");
            var result = _sut.Parse(new[] { "encode", "--bad", "value" });

            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Error, Is.EqualTo("encode parse failed"));
            Assert.That(_encodeParserFake.WasCalled, Is.True);
        }
    }
}