using BusinessLayer.CLI.Utils.Enums;
using BusinessLayer.Services;
using BusinessLayer.Services.Interfaces;

namespace BusinessLayer.CLI.Commands.Encode
{
    public class EncodeCommand : ICliCommand
    {
        public string CommandName => "encode";
        public byte[]? Entropy { get; }
        public ValueFormat Format { get; }
        private readonly IEncodeService encodeService;

        public EncodeCommand(byte[]? entropy, ValueFormat format, IEncodeService encodeService)
        {
            Entropy = entropy;
            Format = format;
            this.encodeService = encodeService;
        }

        public bool Handle()
        {
            var mnemonicSeedRes = encodeService.Encode(Entropy, Format);

            if (mnemonicSeedRes.IsFailed)
            {
                Console.Error.WriteLine(mnemonicSeedRes.Error);
                return false;
            }

            Console.Write(mnemonicSeedRes.Value.ToString());
            return true;
        }
    }
}
