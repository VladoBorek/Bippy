namespace BusinessLayer.CLI.Commands
{
    public interface ICliCommand
    {
        string CommandName { get; }
        bool Handle();
    }
}
