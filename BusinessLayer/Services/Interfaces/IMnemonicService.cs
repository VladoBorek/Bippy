using BusinessLayer.DTOs;
using BusinessLayer.enums;
using FluentResults;


namespace BusinessLayer.Services.Interfaces
{
    public interface IMnemonicService
    {
        Result<MnemonicSeedDTO> GetMnemonicSeed(byte[]? entropy, ValueFormat format);
        public Result<string> Handle(byte[]? entropy, ValueFormat format);
    }
}
