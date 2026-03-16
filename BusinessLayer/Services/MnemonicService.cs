using BusinessLayer.DTOs;
using BusinessLayer.enums;
using BusinessLayer.Services.Interfaces;
using FluentResults;
using NBitcoin;

namespace BusinessLayer.Services
{
    public class MnemonicService : IMnemonicService
    {
        private readonly int DefaultEntropySizeBytes = 32;
        public Result<MnemonicSeedDTO> GetMnemonicSeed(byte[]? entropy, ValueFormat outputFormat)
        {
            var finalEntropy = entropy ?? RandomUtils.GetBytes(DefaultEntropySizeBytes);
            var mnemonic = new Mnemonic(Wordlist.English, finalEntropy);
            var seed = mnemonic.DeriveSeed();

            return Result.Ok(new MnemonicSeedDTO
            {
                Mnemonic = mnemonic,
                Seed = seed,
                IsBinary = outputFormat == ValueFormat.Bin,
            });
        }
    }
}