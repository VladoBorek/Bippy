using BusinessLayer.DTOs;
using ResultPattern;

namespace BusinessLayer.Services.Interfaces
{
    public interface IDeriveService
    {
        Result<DeriveDTO> Derive(byte[] seed, string? path);
    }
}