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
        // ------------------------------
        // Case 1: No args -> HelpCommand
        // ------------------------------
        [Test]
        public void Parse_NoArgs_ReturnsHelpCommand()
        {
            var argParser = new ArgParser(Enumerable.Empty<ICliParser>());

            var result = argParser.Parse(Array.Empty<string>());

            Assert.That(result.IsSuccess, Is.True, result.Error);
            Assert.That(result.Value, Is.InstanceOf<HelpCommand>());
        }

        // -----------------------------------
        // Case 2: "--help" -> HelpCommand
        // -----------------------------------
        [TestCase("--help")]
        public void Parse_HelpFlag_ReturnsHelpCommand(string flag)
        {
            var argParser = new ArgParser(Enumerable.Empty<ICliParser>());

            var result = argParser.Parse(new[] { flag });

            Assert.That(result.IsSuccess, Is.True, result.Error);
            Assert.That(result.Value, Is.InstanceOf<HelpCommand>());
        }

        // --------------------------------------------------------
        // Case 3: Unknown command -> Fail with exact message
        // --------------------------------------------------------
        [Test]
        public void Parse_UnknownCommand_ReturnsFailWithExactMessage()
        {
            var argParser = new ArgParser(Enumerable.Empty<ICliParser>());

            var result = argParser.Parse(new[] { "does-not-exist" });

            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Error, Is.EqualTo("Unrecognized command 'does-not-exist'."));
        }

        // -------------------------------------------------------------------
        // Case 4: Known command -> Delegates to matching ICliParser
        //         and passes only the tail args (args.Skip(1))
        // -------------------------------------------------------------------
        [Test]
        public void Parse_KnownCommand_DelegatesAndReturnsParserResult()
        {
            // Arrange: a dummy ICliCommand to be returned by the parser
            var cmdMock = new Mock<ICliCommand>();
            cmdMock.SetupGet(c => c.CommandName).Returns("dummy");
            cmdMock.Setup(c => c.Handle()).Returns(true);

            // encode parser mock (this is the one that should be called)
            var encodeParser = new Mock<ICliParser>();
            encodeParser.SetupGet(p => p.CommandName).Returns("encode");

            string[]? received = null;
            encodeParser
                .Setup(p => p.Parse(It.IsAny<string[]>()))
                .Callback((string[] a) => received = a)
                .Returns(() => Result.Ok(cmdMock.Object));

            // another parser that must NOT be called
            var otherParser = new Mock<ICliParser>();
            otherParser.SetupGet(p => p.CommandName).Returns("other");
            otherParser
                .Setup(p => p.Parse(It.IsAny<string[]>()))
                .Returns(Result.Fail<ICliCommand>("should not be called"));

            var argParser = new ArgParser(new[] { otherParser.Object, encodeParser.Object });
            var input = new[] { "encode", "--format", "hex", "--entropy", "A1B2" };

            var result = argParser.Parse(input);

            Assert.That(result.IsSuccess, Is.True, result.Error);
            Assert.That(result.Value, Is.SameAs(cmdMock.Object));

            Assert.That(
                received,
                Is.EqualTo(new[] { "--format", "hex", "--entropy", "A1B2" })
            );
            
            encodeParser.Verify(p => p.Parse(It.IsAny<string[]>()), Times.Once);
            otherParser.Verify(p => p.Parse(It.IsAny<string[]>()), Times.Never);
        }

        // -------------------------------------------------------------------
        // Case 5: Known command but parser fails -> ArgParser should propagate
        // -------------------------------------------------------------------
        [Test]
        public void Parse_KnownCommand_WhenParserFails_PropagatesFailure()
        {
            var encodeParser = new Mock<ICliParser>();
            encodeParser.SetupGet(p => p.CommandName).Returns("encode");
            encodeParser
                .Setup(p => p.Parse(It.IsAny<string[]>()))
                .Returns(Result.Fail<ICliCommand>("encode parse failed"));

            var argParser = new ArgParser(new[] { encodeParser.Object });

            var result = argParser.Parse(new[] { "encode", "--bad", "value" });

            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Error, Is.EqualTo("encode parse failed"));

            encodeParser.Verify(p => p.Parse(It.IsAny<string[]>()), Times.Once);
        }
    }
}
