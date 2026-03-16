using BusinessLayer.enums;
using FluentResults;

namespace BusinessLayer.Validators
{
    public class EntropyValidator
    {
        public static Result IsValidEntropy(string entropy, ValueFormat valueFormat)
        {
            if (valueFormat == ValueFormat.Hex)
            {
                if (!Validator.IsHex(entropy))
                {
                    return Result.Fail("Entropy must be a valid hexadecimal string.");
                }

                if (!IsValidHexEntropyLength(entropy))
                {
                    return Result.Fail("Hex entropy must be 32 to 64 characters long, in 8-character increments.");
                }

                return Result.Ok();
            }

            if (!Validator.IsBinary(entropy))
            {
                return Result.Fail("Entropy must be a valid binary string.");
            }

            if (!IsValidBinaryEntropyLength(entropy))
            {
                return Result.Fail("Binary entropy must be 128 to 256 bits long, in 32-bit increments.");
            }

            return Result.Ok();
        }

        private static bool IsValidHexEntropyLength(string entropy)
        {
            return entropy.Length % 8 == 0 && entropy.Length >= 32 && entropy.Length <= 64;
        }

        private static bool IsValidBinaryEntropyLength(string entropy)
        {
            return entropy.Length % 32 == 0 && entropy.Length >= 128 && entropy.Length <= 256;
        }
    }
}
