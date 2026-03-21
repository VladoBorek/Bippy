using BusinessLayer.CLI.Utils.Enums;
using FluentResults;

namespace BusinessLayer.CLI.Validators
{
    public class EncodeValidator
    {
        public static Result IsValidEncode(string? entropy, bool formatProvided, ValueFormat format)
        {
            if (entropy is not null && !formatProvided)
            {
                return Result.Fail("Option '--format' is required when '--entropy' is provided.");
            }

            if (entropy is not null)
            {
                var validationResult = EntropyValidator.IsValidEntropy(entropy, format);
                if (validationResult.IsFailed)
                {
                    return Result.Fail(
                        string.Join(", ", validationResult.Errors.Select(e => e.Message))
                    );
                }
            }

            return Result.Ok();
        }
    }
}
