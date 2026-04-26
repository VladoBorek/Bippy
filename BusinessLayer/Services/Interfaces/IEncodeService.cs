using BusinessLayer.Cli.Utils.Enums;
using BusinessLayer.DTOs;
using ResultPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Services.Interfaces
{
    public interface IEncodeService
    {
        Result<EncodeDTO> Encode(byte[]? entropy, ValueFormat format);
    }
}
