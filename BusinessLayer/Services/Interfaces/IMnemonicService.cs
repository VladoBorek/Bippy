using BusinessLayer.DTOs;
using FluentResults;

namespace BusinessLayer.Services.Interfaces
{
    public interface IMnemonicService
    {
        Result<MnemonicSeedDTO> GetMnemonicSeed(string? entropyHex);
    }
}
