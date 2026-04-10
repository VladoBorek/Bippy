using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs
{
    public class VerifyDTO
    {
        public required bool IsValid { get; set; }

        public override string ToString() => IsValid ? "OK" : "NOK";
    }
}
