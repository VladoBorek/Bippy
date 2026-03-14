using BusinessLayer.DTOs;
using BusinessLayer.Services.Interfaces;
using FluentResults;
using NBitcoin;

namespace BusinessLayer.Services
{
    public class MnemonicService : IMnemonicService
    {
        private readonly int DefaultEntropySize = 32;

        public Result<MnemonicSeedDTO> GetMnemonicSeed(string? entropyHex)
        {
            byte[] entropy;

            if (entropyHex is not null)
            {
                int[] validLengths = { 32, 40, 48, 56, 64 };
                if (!validLengths.Contains(entropyHex.Length) || !IsHex(entropyHex))
                {
                    return Result.Fail(
                        "Error: entropy must be a hex string of 32/40/48/56/64 characters (128–256 bits, step 32 bits)."
                    );
                }
                entropy = Convert.FromHexString(entropyHex);
            }
            else
            {
                entropy = RandomUtils.GetBytes(DefaultEntropySize);
            }

            var mnemonic = new Mnemonic(Wordlist.English, entropy);
            var seed = mnemonic.DeriveSeed();
            // var seed  = mnemonic.DeriveSeed("pass"); // passphrase?

            var result = new MnemonicSeedDTO() { Mnemonic = mnemonic, Seed = seed };

            return Result.Ok(result);
        }

        private static bool IsHex(string s)
        {
            return s.All(c =>
                (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')
            );
        }
    }
}
