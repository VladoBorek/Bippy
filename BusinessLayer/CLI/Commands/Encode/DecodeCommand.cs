using BusinessLayer.CLI.Utils.Enums;
using BusinessLayer.Services.Interfaces;

namespace BusinessLayer.CLI.Commands.Decode
{
    public class DecodeCommand : ICliCommand
    {
        public string CommandName => "decode";
        private readonly string phrase;
        private readonly ValueFormat format;
        private readonly ICommandService commandService;

        public DecodeCommand(string phrase, ValueFormat format, ICommandService commandService)
        {
            this.phrase = phrase;
            this.format = format;
            this.commandService = commandService;
        }

        public bool Handle()
        {
            var decodeRes = commandService.Decode(phrase, format);

            if (decodeRes.IsFailed)
            {
                Console.Error.WriteLine(decodeRes.Error);
                return false;
            }

            Console.Write(decodeRes.Value.ToString());
            return true;
        }
    }
}
