using BusinessLayer.CLI.Utils.Enums;
using BusinessLayer.Services.Interfaces;

namespace BusinessLayer.CLI.Commands.Encode
{
    public class EncodeCommand : ICliCommand
    {
        public string CommandName => "encode";
        public byte[]? Entropy { get; }
        public ValueFormat Format { get; }
        private readonly ICommandService commandService;

        public EncodeCommand(byte[]? entropy, ValueFormat format, ICommandService commandService)
        {
            Entropy = entropy;
            Format = format;
            this.commandService = commandService;
        }

        public bool Handle()
        {
            var res = commandService.Encode(Entropy, Format);

            if (res.IsFailed)
            {
                Console.Error.WriteLine(string.Join(", ", res.Errors.Select(e => e.Message)));
                return false;
            }

            Console.Write(res.Value.ToString());
            return true;
        }
    }
}
