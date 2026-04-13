using BusinessLayer.CLI.Utils.Enums;
using BusinessLayer.CLI.Validators;
using ResultPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Cli.Validators
{
    public static class SeedValidator
    {
        public static Result IsValidSeed(string seed, ValueFormat format)
        {
            if (format == ValueFormat.Hex)
            {
                if (!ValidatorUtils.IsHex(seed))
                    return Result.Fail("Seed must be a valid hexadecimal string.");

                if (seed.Length != 128)
                    return Result.Fail("Hex seed must be exactly 128 characters long.");

                return Result.Ok();
            }

            if (!ValidatorUtils.IsBinary(seed))
                return Result.Fail("Seed must be a valid binary string.");

            if (seed.Length != 512)
                return Result.Fail("Binary seed must be exactly 512 bits long.");

            return Result.Ok();
        }
    }
}
