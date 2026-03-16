using BusinessLayer.DTOs;
using FluentResults;
using BusinessLayer.enums;


namespace BusinessLayer.Services.Interfaces
{
    public interface IMnemonicService
    {
        Result<MnemonicSeedDTO> GetMnemonicSeed(byte[]? entropy, ValueFormat format);
    }
}
