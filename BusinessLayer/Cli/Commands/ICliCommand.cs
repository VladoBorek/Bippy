namespace BusinessLayer.Cli.Commands
{
    public interface ICliCommand
    {
        string CommandName { get; }
        bool Handle();
    }
}
