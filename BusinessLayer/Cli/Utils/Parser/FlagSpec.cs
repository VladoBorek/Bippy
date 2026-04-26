namespace BusinessLayer.Cli.Utils.Parser
{
    public class FlagSpec
    {
        public string Flag { get; init; } = "";
        public bool Required { get; init; }
        public string? Default { get; init; }
    }
}
