using BusinessLayer.CLI.Utils.Enums;
using BusinessLayer.Services.Interfaces;

namespace BusinessLayer.CLI.Commands.Decode
{
    public class DecodeCommand : ICliCommand
    {
        public string CommandName => "decode";
        private readonly string phrase;
        private readonly ValueFormat format;
        private readonly IDecodeService decodeService;

        public DecodeCommand(string phrase, ValueFormat format, IDecodeService decodeService)
        {
            this.phrase = phrase;
            this.format = format;
            this.decodeService = decodeService;
        }

        public bool Handle()
        {
            var decodeRes = decodeService.Decode(phrase, format);

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
