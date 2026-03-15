using BusinessLayer.DTOs;
using BusinessLayer.Services.Interfaces;
using FluentResults;
using NBitcoin;

namespace BusinessLayer.Services
{
    public class MnemonicService : IMnemonicService
    {
        private readonly int DefaultEntropySize = 32;

        public Result<MnemonicSeedDTO> GetMnemonicSeed(string? entropyInput)
        {
            bool IsEntropyBinary = false;
            byte[] entropy;

            if (entropyInput is not null)
            {
                bool hex = IsHex(entropyInput);
                bool bin = IsBinary(entropyInput);

                if (!hex && !bin)
                {
                    return Result.Fail(
                        "Entropy must be a hex string (32–64 hex chars) or binary string (128–256 bits), in steps of 32 bits."
                    );
                }

                if (bin)
                {
                    IsEntropyBinary = true;
                }

                if (!ValidateInputEntropyLength(entropyInput, IsEntropyBinary))
                {
                    return Result.Fail(
                        "Entropy must be a hex string (32–64 hex chars) or binary string (128–256 bits), in steps of 32 bits."
                    );
                }

                if (!IsEntropyBinary)
                {
                    entropy = Convert.FromHexString(entropyInput);
                }
                else
                {
                    entropy = Enumerable
                        .Range(0, entropyInput.Length / 8)
                        .Select(i => Convert.ToByte(entropyInput.Substring(i * 8, 8), 2))
                        .ToArray();
                
                }
            }
            else
            {
                entropy = RandomUtils.GetBytes(DefaultEntropySize);
            }

            var mnemonic = new Mnemonic(Wordlist.English, entropy);
            var seed = mnemonic.DeriveSeed();
            // var seed  = mnemonic.DeriveSeed("pass"); // passphrase?

            var result = new MnemonicSeedDTO()
            {
                Mnemonic = mnemonic,
                Seed = seed,
                IsBinary = IsEntropyBinary,
            };

            return Result.Ok(result);
        }

        private static bool ValidateInputEntropyLength(string entropy, bool isBinary)
        {
            if (isBinary)
            {
                if (entropy.Length % 32 != 0 || entropy.Length < 128 || entropy.Length > 256)
                {
                    return false;
                }
                return true;
            }

            if (entropy.Length % 8 != 0 || entropy.Length < 32 || entropy.Length > 64)
            {
                return false;
            }
            return true;
        }

        private static bool IsHex(string s)
        {
            return s.All(c =>
                (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')
            );
        }

        private static bool IsBinary(string s)
        {
            return s.All(c => c == '0' || c == '1');
        }
    }
}
