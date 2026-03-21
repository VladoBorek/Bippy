using BusinessLayer.CLI.Utils.Enums;
using BusinessLayer.DTOs;
using ResultPattern;

namespace BusinessLayer.Services.Interfaces
{
    public interface ICommandService
    {
        Result<EncodeDTO> Encode(byte[]? entropy, ValueFormat format);
    }
}
