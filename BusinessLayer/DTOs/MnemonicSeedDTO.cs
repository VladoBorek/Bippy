using NBitcoin;

namespace BusinessLayer.DTOs
{
    public class MnemonicSeedDTO
    {
        public required Mnemonic Mnemonic { get; set; }
        public required byte[] Seed { get; set; }
    }
}
