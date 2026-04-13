using BusinessLayer.CLI.Utils.Enums;
using BusinessLayer.DTOs;
using BusinessLayer.Services.Interfaces;
using NBitcoin;
using ResultPattern;
using System.Security.Cryptography;
using System.Text;

namespace BusinessLayer.Services
{
    public class CommandService : ICommandService
    {
        public Result<VerifyDTO> Verify(string phrase, byte[] seed)
        {
            Mnemonic mnemonic;

            try
            {
                mnemonic = new Mnemonic(phrase, Wordlist.English);
            }
            catch (Exception ex)
            {
                return Result.Fail<VerifyDTO>($"Invalid mnemonic: {ex.Message}");
            }

            if (!mnemonic.IsValidChecksum)
            {
                return Result.Fail<VerifyDTO>("Invalid mnemonic checksum.");
            }

            byte[] derivedSeed = mnemonic.DeriveSeed();

            return Result.Ok(new VerifyDTO
            {
                IsValid = CryptographicOperations.FixedTimeEquals(derivedSeed, seed)
            });
        }
    }
}
