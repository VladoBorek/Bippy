using BusinessLayer.CLI.Utils.Enums;
using BusinessLayer.DTOs;
using BusinessLayer.Services.Interfaces;
using FluentResults;
using NBitcoin;

namespace BusinessLayer.Services
{
    public class CommandService : ICommandService
    {
        private readonly int DefaultEntropySizeBytes = 32;

        public Result<EncodeDTO> Encode(byte[]? entropy, ValueFormat format)
        {
            var finalEntropy = entropy ?? RandomUtils.GetBytes(DefaultEntropySizeBytes);
            var mnemonic = new Mnemonic(Wordlist.English, finalEntropy);
            var seed = mnemonic.DeriveSeed();

            return Result.Ok(
                new EncodeDTO
                {
                    Mnemonic = mnemonic,
                    Seed = seed,
                    Format = format,
                }
            );
        }
    }
}
