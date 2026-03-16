using BusinessLayer.enums;

namespace PV286_project.Cli.Commands
{
    public class EncodeCommandParsed : ParsedCommand
    {
        public string? Entropy { get; init; }
        public string? Input { get; init; }
        //public ValueFormat Format { get; init; }
        public ValueFormat Format = ValueFormat.Hex;
        public bool FormatProvided = false;

        public EncodeCommandParsed() : base(CommandType.Encode) { }
    }
}
