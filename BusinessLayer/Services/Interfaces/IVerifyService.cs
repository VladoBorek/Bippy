using BusinessLayer.DTOs;
using ResultPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Services.Interfaces
{
    public interface IVerifyService
    {
        Result<VerifyDTO> Verify(string phrase, byte[] seed);
    }
}
