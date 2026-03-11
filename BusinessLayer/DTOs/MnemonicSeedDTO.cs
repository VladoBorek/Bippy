using NBitcoin;

namespace BusinessLayer.DTOs
{
    public class MnemonicSeedDTO
    {
        public Mnemonic Mnemonic { get; set; }
        public byte[] Seed { get; set; }
    }
}
