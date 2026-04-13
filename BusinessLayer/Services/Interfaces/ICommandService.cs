using BusinessLayer.CLI.Utils.Enums;
using BusinessLayer.DTOs;
using ResultPattern;

namespace BusinessLayer.Services.Interfaces
{
    public interface ICommandService
    {
        Result<DecodeDTO> Decode(string phrase, ValueFormat format);
        Result<VerifyDTO> Verify(string phrase, byte[] seed);
    }
}
